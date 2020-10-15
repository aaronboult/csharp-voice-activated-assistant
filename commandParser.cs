using System;

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

        public void parse(){
            
        }

        public bool isMathematical(){

            string[] commandComponents = this.command.split(" ");

        }

    }

}