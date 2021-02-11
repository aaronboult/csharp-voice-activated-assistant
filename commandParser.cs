using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using MathsAbstractions;
using Managers;

namespace Control{

    class Command{

        public static bool __PerformTest__(string expression, string expected, bool debug = false){

            Command command = new Command(expression, debug);

            string output = command.Execute(true);
            
            if (debug){

                Console.WriteLine($"Expected: {expected}\nOutput: {output}");

            }

            Console.WriteLine($"Expression {expression} Passed: {(output == expected)}");

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

            this.command = command.ToLower();

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
                        parseResult = $"The result of {decodedCommand.expression} is {result}";
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
                    break;

            }

            return parseResult;

        }

        private (bool, string, CommandType) DecodeCommand(){

            CommandType type = CommandType.MATHS_EXPRESSION;

            (bool success, string expression) decodeAttempt = this.TryConstructMathsExpression();
            
            if (!decodeAttempt.success){

                type = CommandType.INFORMATION_LOOKUP;

                decodeAttempt = this.TryParseInformationLookup();

                if (!decodeAttempt.success){
                    
                    type = CommandType.PROGRAM_EXECUTION;

                    decodeAttempt = this.TryParseProgramExecution();

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

            Console.WriteLine(output);

        }

        /// <summary>
        /// Determines whether the command is a mathematical operation
        /// </summary>
        /// <returns>True if the command is a mathematical operation</returns>
        private (bool, string) TryConstructMathsExpression(){

            XmlDocument commandWords = XmlManager.LoadDocument("mathematical-command-words.xml");

            XmlDocument queryIdentifiers = XmlManager.LoadDocument("query-identifiers.xml");

            List<string> filteredCommand = NumericalWordParser.ParseAllNumericalWords(this.command).Split(' ').ToList();

            if (debug){

                DisplayArray(filteredCommand);

            }

            while (filteredCommand.Count > 0){

                if (IsQueryIdentifier(filteredCommand[0], ref queryIdentifiers)){

                    filteredCommand.RemoveAt(0);

                    continue;

                }

                break;

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

                Console.WriteLine($"Equation: '{equation}'");

            }

            return (true, equation);

        }

        private static (bool, string, string) IsMathsKeyword(ref XmlDocument lookup, string value){

            (bool success, XmlNode group, XmlNode keyword) match = XmlManager.GetSecondLevelChild(value, ref lookup);

            XmlNode firstChild = lookup.FirstChild;

            if (match.success){

                if (value == match.keyword.InnerXml){
                    
                    if (match.group.Attributes["symbol"] == null){

                        throw new Exception($"XML lookup group missing symbol attribute at: {lookup.Name}");

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

        private static bool IsQueryIdentifier(string word, ref XmlDocument lookup){

            return XmlManager.GetSecondLevelChild(word, ref lookup).Item1;

        }

        private static bool IsNumerical(string value){

            return double.TryParse(value, out _);

        }

        private (bool, string) TryParseInformationLookup(){

            Console.WriteLine("Parsing information lookup");

            XmlDocument queryIdentifiers = XmlManager.LoadDocument("query-identifiers.xml");

            XmlNode questionWordsNode = XmlManager.GetFirstLevelChild("question-words", ref queryIdentifiers, "name").Item2;

            if (questionWordsNode == null){

                return (false, null);

            }

            XmlNodeList questionWords = questionWordsNode.ChildNodes;

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

                    if (afterParams != "" && afterParams.Length > (tempInformation.Length + value.Length)){

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

                Console.WriteLine($"Information: '{information}'\nSuccess: {informationStarted}");

            }

            return (informationStarted, information);

        }

        private (bool, string) IsInformationLookupKeyword(string word, XmlNodeList lookup){

            (bool success, XmlNode node) match = XmlManager.GetMatchInNodeList(word, lookup);
            
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

            return (false, null);

        }

    }

}