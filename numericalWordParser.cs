using System;
using System.Xml;
using Managers;

namespace MathsAbstractions{

    static class NumericalWordParser{

        private static XmlDocument map;

        static NumericalWordParser(){

            map = XmlManager.LoadDocument("numberParser.xml");

        }

        public static bool __PerformTest__(string phrase, string expected, bool debug = false){

            double result = Parse(phrase, debug);

            double difference = Math.Abs(result - double.Parse(expected));

            if (debug){

                Console.WriteLine($"Parsing: {phrase}\nExpected: {expected}\nGot: {result}\nDifference: {difference}");

            }

            // 0.00000000000000001 is an arbitrary value; a small value is required to be able to differentiate
            // incorrect calculations from floating point error calculations
            if (difference != 0 && difference < 0.00000000000000001){

                Console.WriteLine("Phrase: {phrase}, Passed: True (floating point inaccuracy)");

                return true;

            }
            else{

                Console.WriteLine($"Phrase: {phrase}, Passed: {result.ToString() == expected}");

                return result.ToString() == expected;

            }

        }

        public static string ParseAllNumericalWords(string sentence, bool debug = false){

            string parsedResult = "";

            string[] components = sentence.Split(' ');

            string numbericalPhraseAccumulator = "";

            for (int i = 0 ; i < components.Length ; i++){

                if (IsNumericalWord(components[i])){

                    if (numbericalPhraseAccumulator != ""){

                        numbericalPhraseAccumulator += " ";

                    }

                    numbericalPhraseAccumulator += components[i];

                }
                else{

                    if (numbericalPhraseAccumulator != ""){

                        parsedResult += $"{(parsedResult != "" ? " " : "")}{Parse(numbericalPhraseAccumulator, debug)}";

                        numbericalPhraseAccumulator = "";

                    }

                    parsedResult += $"{(parsedResult != "" ? " " : "")}{components[i]}";

                }

            }

            if (numbericalPhraseAccumulator != ""){

                parsedResult += $" {Parse(numbericalPhraseAccumulator, debug)}";

            }

            return parsedResult;

        }

        private static bool IsNumericalWord(string word){

            return XmlManager.GetSecondLevelChild(word, ref map, "name").Item1;

        }

        public static double Parse(string phrase, bool debug = false){

            (double value, bool set) place = (0, false);
            (double value, bool set) number = (0, false);

            double accumulator = 0;

            bool chainedPlaceValue = false; // A chained place value is something like "hundred thousand"

            bool previousWasTens = false;

            bool multiPlaceValue = false; // Whether a number is constructed using multiple places, i.e. two hundred and ten thousand

            bool nextIsDecimal = false; // Decimal refers to a value after a decimal point, i.e. .1

            bool isNegative = false;

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
                            case 1:
                                isNegative = !isNegative;
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

            return isNegative ? -accumulator : accumulator;

        }

        private static (double, WordGroups) GetMappedWordValue(string word){

            (bool success, XmlNode group, XmlNode word) match = XmlManager.GetSecondLevelChild(word, ref map, "name");

            WordGroups matchedGroup = WordGroups.UNMATCHED;

            double matchedValue = 0;

            if (match.success){

                matchedGroup = (WordGroups)int.Parse(match.group.Attributes["name"].Value);

                if (matchedGroup != WordGroups.CONNECTOR){

                    matchedValue = double.Parse(match.word.Attributes["value"].Value);

                }

            }

            return (matchedValue, matchedGroup);

        }

        enum WordGroups{
            PLACE = 0,
            TENS = 1,
            NUMBER = 2,
            CONNECTOR = 3,
            COMMANDS = 4,
            UNMATCHED
        }

    }

}