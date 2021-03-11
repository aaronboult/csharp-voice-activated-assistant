using System;
using System.Xml;
using Managers;
using MathsAbstractions;

namespace Control{

    static class TestController{

        private static XmlDocument testDocument;

        private delegate bool TestCallback(string input, string output, bool debug);

        static TestController(){

            testDocument = XmlManager.LoadDocument("test-cases.xml");

        }

        public static void RunAllTests(){

            bool passed = true;

            passed = RunCommandParserTests() ? passed : false;

            passed = RunMathTreeTests() ? passed : false;

            passed = RunNumericalWordParserTests() ? passed : false;

            if (passed){

                Console.WriteLine("Passed all tests!");

            }

        }

        public static bool RunCommandParserTests() => PerformTests("command-parser", "input", "output", (TestCallback)Command.__PerformTest__);

        public static bool RunMathTreeTests() => PerformTests("math-tree", "input", "output", (TestCallback)Tree.__PerformTest__);

        public static bool RunNumericalWordParserTests() => PerformTests("numerical-word-parser", "input", "output", (TestCallback)NumericalWordParser.__PerformTest__);

        private static bool PerformTests(string listName, string targetInputAttribute, string targetOutputAttribute, TestCallback callback){

            (bool success, XmlNode nodeList) testMatch = XmlManager.GetFirstLevelChild(listName, ref testDocument, "name");

            if (testMatch.nodeList.Attributes["run"].Value == "false"){

                return true;

            }

            bool passed = true;

            if (testMatch.success){

                foreach (XmlNode node in testMatch.nodeList.ChildNodes){

                    bool success = callback(
                        node.Attributes[targetInputAttribute].Value, 
                        node.Attributes[targetOutputAttribute].Value, 
                        node.Attributes["debug"].Value == "true"
                    );

                    passed = success ? passed : false;

                }

            }

            if (!passed){

                Console.WriteLine($"Test {listName} FAILED");

            }

            return passed;

        }

    }

}