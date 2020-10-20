using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace Control{

    public class Command{

        string command;

        /// <summary>
        /// Constructor for Command
        /// </summary>
        /// <param name="command">The command to carry out</param>
        public Command(string command){

            this.command = command;

        }

        /// <summary>
        /// Parses the respective command and determines what action to carry out
        /// </summary>
        public void Parse(){

            if (this.IsMathematical()){

            }
            
        }

        /// <summary>
        /// Determines whether the command is a mathematical operation
        /// </summary>
        /// <returns>True if the command is a mathematical operation</returns>
        bool IsMathematical(){

            XmlDocument commandWords = new XmlDocument();

            commandWords.Load("mathematical-command-words.xml");

            string[] commandComponents = this.command.Split(' ');

            string equation = ""; // Construct this whenever a keyword or number is found

            int numberOfNumericals = 0;

            foreach(string value in commandComponents){

                if (IsNumerical(value)){

                    numberOfNumericals++;

                    equation += $"{value} ";

                    if (numberOfNumericals > 1){

                        return false;

                    }

                }
                else{

                    numberOfNumericals = 0;

                    if (IsKeyword(commandWords, value)){

                        equation += $"{value} ";

                    }

                }

            }

            return false;

        }

        bool IsKeyword(XmlDocument lookup, string value){

            return false;

        }

        bool IsNumerical(string value){

            return false;

        }

    }

}