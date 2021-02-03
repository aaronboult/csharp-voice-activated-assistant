using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using MathsAbstractions;

namespace Control{

    public class Command{

        public static void __TestExpressionParser__(){

            // __TestExpression__("5 add 6", 11);

            // __TestExpression__("6 over 2", 3);

            // __TestExpression__("8 lots of 3", 24);

            // __TestExpression__("400 times 200", 80_000);

            // __TestExpression__("8 more than 4 over 2 take 5", 5);

            // __TestExpression__("3 lots of open bracket 5 plus 2 close bracket", 21);

        }

        static void __TestExpression__(string expression, double expected, bool debug = false){

            Command command = new Command(expression, debug);

            command.Parse();

            Console.WriteLine($"Expression {expression} Passed: {(command.numericalOutput == expected)}\n\n");

        }

        public double numericalOutput;

        public OutputType type;

        public enum OutputType{
            STRING, NUMERICAL
        }

        string command;

        bool parsed = false;

        bool debug;

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
        public void Parse(){

            if (parsed){
                
                return;

            }

            parsed = true;

            (bool success, string expression) mathsExpression = this.TryConstructMathsExpression();
            
            if (mathsExpression.success){

                Tree tree = new Tree(mathsExpression.expression, this.debug);

                this.numericalOutput = tree.Calculate();
                
                this.type = OutputType.NUMERICAL;

                return;

            }
            
        }

        /// <summary>
        /// Determines whether the command is a mathematical operation
        /// </summary>
        /// <returns>True if the command is a mathematical operation</returns>
        (bool, string) TryConstructMathsExpression(){

            XmlDocument commandWords = new XmlDocument();

            try{

                commandWords.Load("mathematical-command-words.xml");
                
            }
            catch (FileNotFoundException){

                throw new Exception("The mathematical-command-words.xml document is either missing or corrupt.");

            }

            string[] commandComponents = this.command.Split(' ');

            string equation = ""; // Construct this whenever a keyword or number is found

            bool previousWasNumber = false;

            List<string> previousKeywordBuffer = new List<string>(){ "" };

            string keywordFirstWord = "";

            string symbolToAppend = "";

            foreach(string value in commandComponents){

                if (IsNumerical(value)){

                    if (symbolToAppend != ""){

                        symbolToAppend += " ";

                    }

                    equation += symbolToAppend;

                    symbolToAppend = "";

                    if (previousWasNumber){

                        return (false, null);

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

                            previousWasNumber = false;

                            string firstWord = keyword.matchedKeyword.Split(' ')[0];

                            if (keywordFirstWord != firstWord){

                                equation += $"{symbolToAppend}";

                            }
                            
                            symbolToAppend = $" {keyword.operation}";

                            keywordFirstWord = firstWord;

                        }

                    }

                }

            }

            equation += symbolToAppend;

            Console.WriteLine($"Equation: {equation}");

            return (true, equation);

        }

        static (bool, string, string) IsKeyword(ref XmlDocument lookup, string value){

            XmlNode firstChild = lookup.FirstChild;

            foreach (XmlNode group in lookup.FirstChild.ChildNodes){

                foreach (XmlNode keyword in group.ChildNodes){

                    if (value == keyword.InnerXml){
                        
                        if (group.Attributes["symbol"] == null){

                            throw new Exception($"XML lookup group missing symbol attribute at: {lookup.Name}");

                        }

                        string symbol = group.Attributes["symbol"].Value;

                        if (keyword.Attributes["append"] != null){

                            symbol += keyword.Attributes["append"].Value;

                        }

                        symbol += $"_{group.Attributes["priority"].Value}";

                        symbol += $"_{group.Attributes["operandIdentifier"].Value}";

                        return (true, symbol, keyword.InnerXml);

                    }

                }

            }

            return (false, null, null);

        }

        static bool IsNumerical(string value){

            return double.TryParse(value, out _);

        }

    }

}