using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using MathsAbstractions;
using Managers;

namespace Control{

    public class Command{

        public static void __TestExpressionParser__(bool debug = false){

            __TestExpression__("5 add 6", 11, debug);

            __TestExpression__("6 over 2", 3, debug);

            __TestExpression__("8 lots of 3", 24, debug);

            __TestExpression__("400 times 200", 80_000, debug);

            __TestExpression__("8 more than 4 over 2 take 5", 5, debug);

            __TestExpression__("3 lots of open bracket 5 plus 2 close bracket", 21, debug);

            __TestExpression__("What is 2 plus 3", 5, debug);

            __TestExpression__("What is four plus three", 7, debug);

            __TestExpression__("negative five minus three", -8, debug);

            __TestExpression__("four minus negative two", 6, debug);

            __TestExpression__("eight squared", 64);

            __TestExpression__("four to the three", 64);

            __TestExpression__("log ten", 1);

            __TestExpression__("log one", 0);

            __TestExpression__("log base two of eight", 3);

            // __TestExpression__("ln zero", 1);

        }

        static void __TestExpression__(string expression, double expected, bool debug = false){

            Command command = new Command(expression, debug);

            string output = command.Execute();

            string expectedOutput = expected.ToString();

            if (!debug){

                expectedOutput = $"The result of {expression} is {expected}";

            }

            Console.WriteLine($"Expression {expression} Passed: {(output == expectedOutput)}");

        }

        private string command;

        private string parseResult = "That command was not recognised";

        private bool parsed = false;

        private bool debug;

        /// <summary>
        /// Constructor for Command
        /// </summary>
        /// <param name="command">The command to carry out</param>
        public Command(string command, bool debug = false){

            this.command = command;

            this.debug = debug;

        }

        /// <summary>
        /// Parses the respective command and determines what action to carry out
        /// </summary>
        public string Execute(){

            if (parsed){
                
                return parseResult;

            }

            parsed = true;

            (bool success, string expression) mathsExpression = this.TryConstructMathsExpression();
            
            if (mathsExpression.success){

                Tree tree = new Tree(mathsExpression.expression, this.debug);

                double result = tree.Calculate();

                if (debug){

                    parseResult = result.ToString();

                }
                else{

                    parseResult = $"The result of {command} is {result}";

                }

            }

            return parseResult;

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
        (bool, string) TryConstructMathsExpression(){

            XmlDocument commandWords = XmlManager.LoadDocument("mathematical-command-words.xml");

            XmlDocument queryIdentifiers = XmlManager.LoadDocument("query-identifiers.xml");

            List<string> filteredCommand = NumericalWordParser.ParseAllNumericalWords(this.command).Split(' ').ToList();

            if (debug){

                DisplayArray(filteredCommand);

            }

            while (filteredCommand.Count > 0){

                if (IsQueryIdentifier(filteredCommand[0].ToLower(), ref queryIdentifiers)){

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

            List<string> previousKeywordBuffer = new List<string>(){ "" };

            string keywordFirstWord = "";

            string symbolToAppend = "";

            bool dualOperator = false;

            foreach(string value in commandComponents){

                if (IsNumerical(value)){

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

                    previousKeywordBuffer = new List<string>();

                    equation += value;

                }
                else{

                    previousKeywordBuffer.Add(value);

                    for (int i = 0 ; i < previousKeywordBuffer.Count ; i++){

                        if (previousKeywordBuffer[i] != value){

                            previousKeywordBuffer[i] += $" {value}";

                        }

                        (bool isKeyword, string operation, string matchedKeyword) keyword = IsKeyword(ref commandWords, previousKeywordBuffer[i]);

                        if (keyword.isKeyword){

                            string operandIdentifier = keyword.operation.Split('_')[2];

                            dualOperator = operandIdentifier == "1" || operandIdentifier == "2";

                            previousWasNumber = false;

                            string firstWord = keyword.matchedKeyword.Split(' ')[0];

                            if (keywordFirstWord != firstWord){

                                equation += $"{symbolToAppend}";

                            }
                            
                            symbolToAppend = $"{(equation != "" ? " " : "")}{keyword.operation}";

                            keywordFirstWord = firstWord;

                        }

                    }

                }

            }

            equation += symbolToAppend;

            if (debug){

                Console.WriteLine($"Equation: '{equation}'");

            }

            return (true, equation);

        }

        static (bool, string, string) IsKeyword(ref XmlDocument lookup, string value){

            (bool success, XmlNode group, XmlNode keyword) match = XmlManager.HasSecondLevelChild(value, ref lookup);

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

        static bool IsQueryIdentifier(string word, ref XmlDocument lookup){

            return XmlManager.HasSecondLevelChild(word, ref lookup).Item1;

        }

        static bool IsNumerical(string value){

            return double.TryParse(value, out _);

        }

    }

}