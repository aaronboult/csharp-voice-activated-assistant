using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Managers;
using GUI;

namespace Parsing{

    class Command{

        public static bool __PerformTest__(string expression, string expected, bool debug = false){

            Command command = new Command(expression, debug);

            string output = command.Execute(true);
            
            if (debug){

                GUIController.LogOutput($"Expected: {expected}\nOutput: {output}");

            }

            GUIController.LogOutput($"Expression {expression} Passed: {(output == expected)}");

            return output == expected;

        }

        private string command;

        private string parseResult = "That command was not recognised";

        private bool parsed = false;

        private bool debug;

        enum CommandType{
            MATHS_EXPRESSION,
            INFORMATION_LOOKUP,
            PROGRAM_EXECUTION,
            NO_COMMAND
        }

        /// <summary>
        /// Constructor for Command
        /// </summary>
        /// <param name="command">The command to carry out</param>
        public Command(string command, bool debug = false){

            this.command = command.Trim().ToLower();

            string tempCommand = this.command;

            while ((this.command = this.command.Replace("  ", " ")) != tempCommand){

                tempCommand = this.command;

            }

            this.debug = debug;

        }

        /// <summary>
        /// Parses the respective command and determines what action to carry out
        /// </summary>
        public string Execute(bool test = false){

            if (parsed){
                
                return parseResult;

            }

            parsed = true;

            (bool success, string expression, CommandType type) decodedCommand = this.DecodeCommand();

            switch (decodedCommand.type){

                case CommandType.MATHS_EXPRESSION:
                    Tree tree = new Tree(decodedCommand.expression, this.debug);
                    double result = tree.Calculate();
                    if (test){
                        parseResult = result.ToString();
                    }
                    else{
                        parseResult = $"The result of {command} is {result}";
                    }
                    break;
                
                case CommandType.INFORMATION_LOOKUP:
                    InformationGrabber grabber = new InformationGrabber();
                    if (test){
                        parseResult = decodedCommand.expression;
                    }
                    else{
                        string grabbedData = grabber.SearchForTerm(decodedCommand.expression);
                        if (grabbedData == ""){
                            parseResult = $"I couldn't find any information about: {decodedCommand.expression}";
                        }
                        else{
                            parseResult = $"Here's what I could source from Wikipedia about {decodedCommand.expression}: {grabbedData}";
                        }
                    }
                    break;

                case CommandType.PROGRAM_EXECUTION:
                    bool success = ProgramManager.OpenProgram(decodedCommand.expression);
                    parseResult = $"{decodedCommand.expression} opened successfully: {success}";
                    break;

            }

            return parseResult;

        }

        private (bool, string, CommandType) DecodeCommand(){

            CommandType type = CommandType.MATHS_EXPRESSION;

            (bool success, string expression) decodeAttempt = this.TryConstructMathsExpression();
            
            if (!decodeAttempt.success){

                type = CommandType.PROGRAM_EXECUTION;

                decodeAttempt = this.TryParseProgramExecution();

                if (!decodeAttempt.success){
                    
                    type = CommandType.INFORMATION_LOOKUP;

                    decodeAttempt = this.TryParseInformationLookup();

                    if (!decodeAttempt.success){

                        type = CommandType.NO_COMMAND;

                    }

                }

            }

            if (decodeAttempt.success){

                return (true, decodeAttempt.expression, type);

            }

            return (false, null, CommandType.NO_COMMAND);

        }

        // Debug functions
        public static void DisplayArray(List<string> list) => DisplayArray(list.ToArray());

        public static void DisplayArray(string[] array){

            string output = "[";

            for (int i = 0 ; i < array.Length ; i++){

                output += array[i];

                if (i < array.Length - 1){

                    output += ",";

                }

            }

            output += "]";

            GUIController.LogOutput(output);

        }

        /// <summary>
        /// Determines whether the command is a mathematical operation
        /// </summary>
        /// <returns>True if the command is a mathematical operation</returns>
        private (bool, string) TryConstructMathsExpression(){

            XmlDocument commandWords = XmlManager.LoadDocument("mathematical-command-words.xml");

            XmlNodeList mathsIdentifiers = XmlManager.GetFirstLevelNodeChildren("query-identifiers.xml", "maths-question-words");

            if (mathsIdentifiers == null){

                return (false, null);

            }

            List<string> filteredCommand = NumericalWordParser.ParseAllNumericalWords(this.command).Split(' ').ToList();

            if (debug){

                DisplayArray(filteredCommand);

            }

            int identifierOffset = 0;

            string currentIdentifier = "";

            while (filteredCommand.Count > 0 && identifierOffset < filteredCommand.Count){

                if (currentIdentifier != ""){

                    currentIdentifier += " ";

                }

                currentIdentifier += filteredCommand[identifierOffset];

                if (IsQueryIdentifier(currentIdentifier, mathsIdentifiers)){
                    
                    for (int i = 0 ; i < identifierOffset + 1 ; i++){

                        filteredCommand.RemoveAt(0);

                    }

                    identifierOffset = 0;

                    currentIdentifier = "";

                    continue;

                }
                
                identifierOffset++;

            }

            if (debug){

                DisplayArray(filteredCommand);

            }

            string[] commandComponents = filteredCommand.ToArray();

            string equation = ""; // Construct this whenever a keyword or number is found

            bool previousWasNumber = false;

            List<string> previousWords = new List<string>();

            string keywordFirstWord = "";

            string symbolToAppend = "";

            bool dualOperator = false;

            bool awaitingMatch = false;

            foreach(string value in commandComponents){

                if (IsNumerical(value)){

                    if (awaitingMatch){

                        return (false, null);

                    }

                    if (symbolToAppend != ""){

                        symbolToAppend += " ";

                    }

                    equation += $"{symbolToAppend}";

                    symbolToAppend = "";

                    if (previousWasNumber){

                        if (dualOperator){

                            equation += " ";

                        }
                        else{

                            return (false, null);

                        }

                    }

                    previousWasNumber = true;

                    previousWords = new List<string>();

                    equation += value;

                }
                else{

                    previousWords.Add(value);

                    bool keywordMatched = false;

                    for (int i = 0 ; i < previousWords.Count ; i++){

                        if (previousWords[i] != value){

                            previousWords[i] += $" {value}";

                        }

                        (bool isKeyword, string operation, string matchedKeyword) keyword = IsMathsKeyword(ref commandWords, previousWords[i]);

                        if (keyword.isKeyword){
                            
                            keywordMatched = true;

                            awaitingMatch = false;

                            string operandIdentifier = keyword.operation.Split('_')[2];

                            dualOperator = operandIdentifier == "1" || operandIdentifier == "2";

                            previousWasNumber = false;

                            string firstWord = keyword.matchedKeyword.Split(' ')[0];

                            if (keywordFirstWord != firstWord){

                                equation += $"{symbolToAppend}";

                            }
                            
                            symbolToAppend = $"{(equation != "" ? " " : "")}{keyword.operation}";

                            keywordFirstWord = firstWord;

                            break;

                        }

                    }
                    
                    if (!keywordMatched){

                        awaitingMatch = true;

                    }

                }

            }

            if (awaitingMatch){

                return (false, null);

            }

            equation += symbolToAppend;

            if (debug){

                GUIController.LogOutput($"Equation: '{equation}'");

            }

            return (true, equation);

        }

        private static (bool, string, string) IsMathsKeyword(ref XmlDocument lookup, string value){

            (bool success, XmlNode group, XmlNode keyword) match = XmlManager.GetSecondLevelChild(value, ref lookup, matchInnerXml: true);

            XmlNode firstChild = lookup.FirstChild;

            if (match.success){
                
                if (value == match.keyword.InnerXml){
                    
                    if (match.group.Attributes["symbol"] == null){

                        throw new Exception($"XML lookup group missing symbol attribute at: {lookup.InnerXml}");

                    }

                    string symbol = "";

                    if (match.group.Attributes["loperand"] != null){

                        symbol += $"{match.group.Attributes["loperand"].Value} ";

                    }

                    symbol += match.group.Attributes["symbol"].Value;

                    if (match.keyword.Attributes["append"] != null){

                        symbol += match.keyword.Attributes["append"].Value;

                    }

                    symbol += $"_{match.group.Attributes["priority"].Value}";

                    symbol += $"_{match.group.Attributes["operandIdentifier"].Value}";

                    if (match.group.Attributes["roperand"] != null){

                        symbol += $" {match.group.Attributes["roperand"].Value}";

                    }

                    return (true, symbol, match.keyword.InnerXml);

                }

            }

            return (false, null, null);

        }

        private static bool IsQueryIdentifier(string word, XmlNodeList lookup){

            return XmlManager.GetMatchInNodeList(word, lookup, matchInnerXml: true).Item1;

        }

        private static bool IsNumerical(string value){

            return double.TryParse(value, out _);

        }

        private (bool, string) TryParseInformationLookup(){

            GUIController.LogOutput("Parsing information lookup");

            XmlNodeList questionWords = XmlManager.GetFirstLevelNodeChildren("query-identifiers.xml", "question-words");

            if (questionWords == null){

                return (false, null);

            }

            string[] commandComponents = command.Split(' ');

            string information = "";

            string tempInformation = "";

            List<string> previousWords = new List<string>();

            bool informationStarted = false;

            string afterParams = "";

            foreach (string value in commandComponents){
                
                if (!informationStarted){

                    previousWords.Add("");

                    (bool success, string afterParams) lookupKeywordMatch = (false, null);

                    for (int i = 0 ; i < previousWords.Count ; i++){

                        previousWords[i] += $"{(previousWords[i] == "" ? "" : " ")}{value}";

                        if (!lookupKeywordMatch.success){

                            lookupKeywordMatch = IsInformationLookupKeyword(previousWords[i], questionWords);

                        }

                    }

                    if (lookupKeywordMatch.success){

                        afterParams = lookupKeywordMatch.afterParams;

                    }
                    else{

                        information += value;

                        informationStarted = true;

                    }

                }
                else{

                    if (afterParams != "" && afterParams.Length >= (tempInformation.Length + value.Length)){

                        if (afterParams.Substring(0, tempInformation.Length + value.Length) == tempInformation + value){

                            if (tempInformation + value == afterParams){

                                break;

                            }
                            else{

                                tempInformation += $"{(tempInformation == "" ? "" : " ")}{value}";

                            }

                            continue;

                        }

                    }

                    if (information != "" && tempInformation != ""){

                        tempInformation = $" {tempInformation}";

                    }

                    information += $"{tempInformation}{(information == "" ? "" : " ")}{value}";

                    tempInformation = "";

                }

            }

            if (debug){

                GUIController.LogOutput($"AfterParams: '{afterParams}'\nInformation: '{information}'\nSuccess: {informationStarted}");

            }

            return (informationStarted, information);

        }

        private (bool, string) IsInformationLookupKeyword(string word, XmlNodeList lookup){

            (bool success, XmlNode node) match = XmlManager.GetMatchInNodeList(word, lookup, matchInnerXml: true);
            
            if (match.success){

                string afterParamWord = "";

                if (match.node.Attributes["after-param-word"] != null){

                    afterParamWord = match.node.Attributes["after-param-word"].Value;

                }

                return (match.success, afterParamWord);

            }

            return (false, null);

        }

        private (bool, string) TryParseProgramExecution(){

            XmlNodeList executionKeywords = XmlManager.GetFirstLevelNodeChildren("query-identifiers.xml", "program-execution-words");

            if (executionKeywords == null){
                
                return (false, null);

            }

            string[] commandComponents = command.Split(' ');

            bool nameStarted = false;

            string name = "";

            foreach (string value in commandComponents){

                if (nameStarted){

                    name += $"{(name == "" ? "" : " ")}{value}";

                }
                else if (IsProgramExecutionKeyword(value, executionKeywords)){

                    nameStarted = true;

                }

            }

            return (name != "", name);

        }

        private bool IsProgramExecutionKeyword(string value, XmlNodeList lookup){

            return XmlManager.GetMatchInNodeList(value, lookup, matchInnerXml: true).Item1;

        }

    }

}