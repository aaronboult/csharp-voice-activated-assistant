using System;
using MathsAbstractions;

namespace Control
{
    class Program{

        private static void Main(string[] args){

            // RunTests();

            VoiceController voice = new VoiceController();

            voice.Listen();

        }

        private static void RunTests(){

            // Command.__TestExpressionParser__();

            // Tree.__TestTreeConstructor__();

            // NumericalWordParser.__TestWordParser__();

        }

    }
}