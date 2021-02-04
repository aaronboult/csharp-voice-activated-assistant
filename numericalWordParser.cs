using System;
using System.IO;
using System.Xml;

namespace MathsAbstractions{

    static class NumericalWordParser{

        private static XmlDocument map;

        static NumericalWordParser(){

            map = new XmlDocument();

            try{

                map.Load("numberParser.xml");
                
            }
            catch (FileNotFoundException){

                throw new Exception("The numberParser.xml document is either missing or corrupt.");

            }

        }

        public static void __TestWordParser__(){

            __PerformTest__("one", 1);

            __PerformTest__("twenty three", 23);

            __PerformTest__("five hundred and seven", 507);

            __PerformTest__("two hundred thousand", 200_000);

            __PerformTest__("six hundred and ninety four thousand", 694_000);

            __PerformTest__("twenty two thousand", 22_000);

            __PerformTest__("five thousand and one", 5001);

            __PerformTest__("a hundred and two", 102);

            __PerformTest__("six hundred and one thousand", 601_000);

            __PerformTest__("two million five thousand and six", 2_005_006);

            __PerformTest__("five point four", 5.4);

            __PerformTest__("four point fifty three", 4.53);

            __PerformTest__("one point twelve", 1.12);

            __PerformTest__("ten point seventy five", 10.75);

            __PerformTest__("one hundred and seven point three six five", 107.365);

            __PerformTest__("two point four thousand point one", 2400.1);

            __PerformTest__("two point five million and twelve", 2_500_012);

            __PerformTest__("twelve hundred", 1200);

            __PerformTest__("point five", 0.5);

            __PerformTest__("two hundred point one thousand", 200_100);

            __PerformTest__("three point one four one five nine two six five four", 3.141592654);

            __PerformTest__("point nine million", 900_000);

            __PerformTest__("point five hundred and six", 56);

            __PerformTest__("point zero zero zero seven", 0.0007);

            __PerformTest__("one hundred and ninety thousand", 190_000);

            __PerformTest__("five hundred thirty", 530);

            __PerformTest__("two million eighty four thousand", 2_084_000);

            __PerformTest__("five eight six", 586);

        }

        private static void __PerformTest__(string phrase, double expected, bool debug = false){

            double result = Parse(phrase, debug);

            double difference = Math.Abs(result - expected);

            if (debug){

                Console.WriteLine($"Parsing: {phrase}\nExpected: {expected}\nGot: {result}\nDifference: {difference}");

            }

            // 0.00000000000000001 is an arbitrary value; a small value is required to be able to differentiate
            // incorrect calculations from floating point error calculations
            if (difference != 0 && difference < 0.00000000000000001){

                Console.WriteLine("Phrase: {phrase}, Passed: True (floating point inaccuracy)");

            }
            else{

                Console.WriteLine($"Phrase: {phrase}, Passed: {result == expected}");

            }

        }

        public static double Parse(string phrase, bool debug = false){

            (double value, bool set) place = (0, false);
            (double value, bool set) number = (0, false);

            double accumulator = 0;

            bool chainedPlaceValue = false; // A chained place value is something like "hundred thousand"

            bool previousWasTens = false;

            bool multiPlaceValue = false; // Whether a number is constructed using multiple places, i.e. two hundred and ten thousand

            bool nextIsDecimal = false; // Decimal refers to a value after a decimal point, i.e. .1

            (int count, bool on) decimalToggle = (0, false);

            string[] words = phrase.Split(' ');

            // Iterate over each component in the number and determine whether its a number, place or otherwise

            for (int i = 0 ; i < words.Length ; i++){

                (double value, WordGroups group) match = GetMappedWordValue(words[i]);

                int nextPlaceCheckOffset = 2;

                nextIsDecimal = false;

                if (debug){

                    Console.WriteLine(multiPlaceValue);

                }

                if (i + 1 < words.Length){

                    (double value, WordGroups group) nextMatch = GetMappedWordValue(words[i + 1]);

                    chainedPlaceValue = match.group == WordGroups.PLACE && nextMatch.group == WordGroups.PLACE; // If next word is a place value, it is a chained place value

                    nextIsDecimal = nextMatch.group == WordGroups.COMMANDS && nextMatch.value == 0; // nextMatch.value is 0 when indicating a decimal has started

                    // The number could be two hundred and five thousand, therefore the 'and' needs to be considered in the offset
                    nextPlaceCheckOffset = nextMatch.group == WordGroups.CONNECTOR ? 3 : 2;

                    if (i + nextPlaceCheckOffset < words.Length){

                        // The upcoming match is the word match one place before where the next place value is expected
                        (double value, WordGroups group) upcomingMatch = GetMappedWordValue(words[i + nextPlaceCheckOffset]);

                        if (upcomingMatch.group != WordGroups.PLACE){
                            
                            upcomingMatch = GetMappedWordValue(words[i + nextPlaceCheckOffset - 1]);

                            if (upcomingMatch.group == WordGroups.TENS){

                                // Increase the offset by 1 if the next number contains both a ten and another number, i.e. twenty five
                                nextPlaceCheckOffset++;

                            }

                        }

                    }

                }

                if (debug){

                    Console.WriteLine(nextPlaceCheckOffset);

                    Console.WriteLine(decimalToggle);

                }

                if (i + nextPlaceCheckOffset < words.Length && match.group == WordGroups.PLACE){

                    (double value, WordGroups group) upcomingMatch = GetMappedWordValue(words[i + nextPlaceCheckOffset]);

                    if (debug){

                        Console.WriteLine($"UpcommingMatch: {upcomingMatch}");

                    }

                    // If the next number is succeeded by a place value, and the next place value is of a lesser order,
                    // consider the two values to be connected. E.g. five million and seventy thousand
                    multiPlaceValue = upcomingMatch.group == WordGroups.PLACE && (upcomingMatch.value > match.value);

                }

                if (debug){

                    Console.WriteLine(match);

                }

                switch (match.group){

                    case WordGroups.CONNECTOR:
                        continue;

                    case WordGroups.COMMANDS:
                        // Determine which command is being used
                        switch (match.value){
                            case 0:
                                decimalToggle = (1, true);
                                break;
                        }
                        break;

                    case WordGroups.PLACE:
                        if (place.set && !multiPlaceValue){
                            place.value *= match.value; // Used with adjacent place values, e.g. hundred thousand
                        }
                        else{
                            place = (match.value, true);
                        }
                        previousWasTens = false;
                        decimalToggle = (0, false);
                        break;

                    case WordGroups.TENS:
                        if (decimalToggle.on){
                            // Append the new decimal digit by using powers of 0.1 to move down the place-values
                            number.value += match.value * Math.Pow(0.1, decimalToggle.count + 1);
                            number.set = true;
                            decimalToggle.count++;
                        }
                        else if (multiPlaceValue){
                            // Move the already-assigned value up the place values before appending the new number
                            // Stops "two million two hundred" losing the "two millions"s significance
                            number = ((number.value * place.value) + match.value, true);
                            place = (0, false);
                        }
                        else{
                            number = (match.value + number.value, true);
                        }
                        previousWasTens = true;
                        break;

                    case WordGroups.NUMBER:
                        if (decimalToggle.on){
                            // If a place value is assigned, move the value into its correct place-value position
                            // Before appending decimals in order to maintain their significance
                            if (place.set){
                                number.value *= place.value;
                                place = (0, false);
                            }
                            // Append the new decimal digit by using powers of 0.1 to move down the place-values
                            number.value += match.value * Math.Pow(0.1, decimalToggle.count + (int)(match.value / 10));
                            number.set = true;
                            decimalToggle.count++;
                        }
                        else if (multiPlaceValue){
                            // Shift the already-set value into its correct place-value position before appending the new value
                            number = ((number.value * (place.set ? place.value : 1)) + match.value, true);
                        }
                        else if (number.set && !previousWasTens){
                            // Allows for saying consecutive digits, such as "one four three"
                            accumulator += number.value;
                            accumulator *= 10;
                            number.value = match.value;
                        }
                        else{
                            number = (match.value + number.value, true);
                        }
                        previousWasTens = false;
                        break;

                    default:
                        break;

                }

                if (debug){

                    Console.WriteLine($"\n\n\n\nNumber: {number}\nPlace: {place}\nAccumulator: {accumulator}\n\n\n\n");

                }

                // Separated conditions to avoid having an absurdly long if(...)

                bool lastIteration = i + 1 == words.Length;

                bool placeValueCondition = (!chainedPlaceValue || lastIteration) && (!multiPlaceValue || lastIteration);

                bool decimalCondition = !nextIsDecimal && (!decimalToggle.on || lastIteration);

                if (number.set && place.set && placeValueCondition && decimalCondition){

                    if (debug){

                        Console.WriteLine(
                            $"Adding to accumulator\nNumber: {number}\nPlace: {place}\nChained Place: {chainedPlaceValue}\nMulti Place: {multiPlaceValue}\nLast: {lastIteration}"
                        );

                    }

                    accumulator += number.value * place.value;

                    place = number = (0, false);

                }

            }

            if (place.set){

                accumulator += place.value;

            }

            if (number.set){

                accumulator += number.value;

            }

            if (debug){

                Console.WriteLine(accumulator);
                
            }

            return accumulator;

        }

        private static (double, WordGroups) GetMappedWordValue(string word){

            (bool success, double value) match;

            if ((match = ContainsWord(word, "connectors")).success){

                return (0, WordGroups.CONNECTOR);

            }
            else if ((match = ContainsWord(word, "places")).success){

                return (match.value, WordGroups.PLACE);

            }
            else if ((match = ContainsWord(word, "tens")).success){

                return (match.value, WordGroups.TENS);

            }
            else if ((match = ContainsWord(word, "numbers")).success){

                return (match.value, WordGroups.NUMBER);

            }
            else if ((match = ContainsWord(word, "commands")).success){

                return (match.value, WordGroups.COMMANDS);

            }

            return (0, WordGroups.UNMATCHED);

        }

        private static (bool, double) ContainsWord(string word, string groupName){

            XmlNodeList children = map.GetElementsByTagName(groupName).Item(0).ChildNodes;

            for (int i = 0 ; i < children.Count ; i++){

                if (children[i].Attributes["name"].Value == word){

                    double value = 0;

                    if (children[i].Attributes["value"] != null){

                        value = double.Parse(children[i].Attributes["value"].Value);

                    }

                    return (true, value);

                }

            }

            return (false, 0);

        }

        enum WordGroups{
            PLACE,
            TENS,
            NUMBER,
            CONNECTOR,
            COMMANDS,
            UNMATCHED
        }

    }

}