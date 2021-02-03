using System;
using System.Linq;
using System.Collections.Generic;

namespace MathsAbstractions{

    /// <summary>
    /// A collection of ordered components in an expression based on BIDMAS
    /// </summary>
    public class Tree{

        private bool debug = false;

        private List<Operation> expression;

        private int entryIndex = 0;

        /// <summary>
        /// Perform tests to check whether the parser is working properly
        /// </summary>
        public static void __TestTreeConstructor__(){

            // __PerformTest__("5 +_0_0 5", 10); // 5 + 5

            // __PerformTest__("3 *_1_0 3 -_0_0 2 *_1_0 4", 1); // 3 * 3 - 2 * 4 <---- stackoverflow exception

            // __PerformTest__("9 -_0_0 8 *_1_0 7 +_0_0 6 /_1_0 2", -44); // 9 - 8 * 7 + 6 / 2

            // __PerformTest__("2 *_1_0 3 *_1_0 4 *_1_0 5 *_1_0 6 *_1_0 7", 5040); // 2 * 3 * 4 * 5 * 6 * 7

            // __PerformTest__("10 -_0_0 1 *_1_0 2 *_1_0 3 +_0_0 20", 24);

            // __PerformTest__("-6 +_0_0 3", -3); // -6 + 3

            // __PerformTest__("3 -inv_3_0 6", 3); // 6 - 3

            // __PerformTest__("2 *_1_0 (_3_4 3 +_0_0 5 )_3_5", 16); // 2 * ( 3 + 5 )

            // __PerformTest__("2 *_1_0 (_3_4 1 +_0_0 2 )_3_5 +_0_0 1", 7); // 2 * ( 1 + 2 ) + 1

            // __PerformTest__("2 *_1_0 (_3_4 1 +_0_0 2 *_1_0 4 )_3_5 +_0_0 7", 18 + 7); // 2 * ( 1 + 2 * 4 )

            // __PerformTest__("3 *_1_0 (_3_4 2 *_1_0 (_3_4 3 +_0_0 5 )_3_5 )_3_5", 48); // 3 * ( 2 * ( 3 + 5 ) )

            // __PerformTest__("2 *_1_0 (_3_4 6 /_1_0 (_3_4 1 +_0_0 (_3_4 4 /_1_0 2 )_3_5 )_3_5 )_3_5", 4); // 2 * ( 6 / ( 1 + ( 4 / 2 ) ) )

            // __PerformTest__("3 *_1_0 (_3_4 4 /_1_0 2 )_3_5 -_0_0 1", 5); // 3 * ( 4 / 2 ) - 1

            // __PerformTest__("2 *_1_0 (_3_4 3 *_1_0 (_3_4 1 +_0_0 1 )_3_5 -_0_0 2 )_3_5", 8); // 2 * ( 3 * ( 1 + 1 ) - 2 )

            // __PerformTest__("1 +_0_0 3 ^_2_0 2", 10);

        }

        /// <summary>
        /// Perform a test on the parser
        /// </summary>
        /// <param name="expression">The expression to parse</param>
        /// <param name="expected">The expected result</param>
        static void __PerformTest__(string expression, double expected, bool debug = false){

            Tree testTree = new Tree(expression, debug); // Expect 9

            Console.WriteLine($"Expression {expression} Passed: {(testTree.Calculate() == expected)}\n\n");

        }

        /// <summary>
        /// Construct a new Tree object
        /// </summary>
        /// <param name="equation">The equation to parse</param>
        /// <param name="debug">Whether or not to display debug information</param>
        public Tree(string equation, bool debug = false){

            this.debug = debug;

            Operation.debug = debug;

            string[] components = equation.Split(' ');

            int[] operationIndexs = ProfileOperators(ref components);

            Stack<(int, int)> bracketQueue = new Stack<(int, int)>();

            List<int> operatorsToRemove = new List<int>();

            Stack<string> operands = new Stack<string>();

            for (int i = components.Length - 1 ; i >= 0 ; i--){ // Work backwards due to first in, last out

                if (Array.IndexOf(operationIndexs, i) == -1){

                    if (debug){
                        
                        Console.WriteLine($"Pushing {components[i]} to operand stack");

                    }

                    operands.Push(components[i]);

                }

            }

            bool settingLeft = true;

            int rightConsumerIndex = -1; // -1 denotes there is no set rightConsumer

            for (int i = 0 ; i < expression.Count ; i++){

                if (debug){

                    for (int o = 0 ; o < expression.Count ; o++){

                        Console.WriteLine(expression[o].ToString());
                        Console.WriteLine("\n");

                    }

                    Console.WriteLine("\n\n\n\n");

                }

                if (expression[i].operandData == Operation.OperatorInfo.BRACKETOPEN){

                    bracketQueue.Push((i, rightConsumerIndex));

                    rightConsumerIndex = -1;

                    settingLeft = true;

                    if (expression.Count - 1 > i){

                        expression[bracketQueue.Peek().Item1].right = new Operation.Operand(expression[i + 1]);

                    }

                    continue;

                }
                else if (expression[i].operandData == Operation.OperatorInfo.BRACKETCLOSE){

                    rightConsumerIndex = bracketQueue.Pop().Item2;

                    operatorsToRemove.Add(i);

                    settingLeft = false;

                    if (expression.Count - 1 > i){

                        expression[i + 1].left = new Operation.Operand(expression[rightConsumerIndex]);
                        
                        if (bracketQueue.Count > 0){

                            expression[bracketQueue.Peek().Item1].right = new Operation.Operand(expression[i + 1]);

                        }

                    }

                    continue;

                }

                if (settingLeft){

                    expression[i].left = new Operation.Operand(operands.Pop());

                }

                settingLeft = true;

                if (expression.Count - 1 > i){

                    if (expression[i].priority >= expression[i + 1].priority){ // Consume as many operands as needed; assign self to next's left

                        if (rightConsumerIndex < 0 || expression[i].priority == expression[i + 1].priority){

                            expression[i + 1].left = new Operation.Operand(expression[i]);

                            if (rightConsumerIndex >= 0){ // If there is a rightConsumer set, update where it's right reference points to

                                expression[rightConsumerIndex].right = new Operation.Operand(expression[i + 1]);

                            }

                        }
                        else{

                            expression[i + 1].left = new Operation.Operand(expression[rightConsumerIndex]);

                            rightConsumerIndex = -1;

                        }

                        settingLeft = false;

                    }
                    else{
                        
                        if (expression[i + 1].operandData != Operation.OperatorInfo.BRACKETCLOSE){

                            expression[i].right = new Operation.Operand(expression[i + 1]);

                            rightConsumerIndex = i;

                            continue;

                        }

                    }

                }
                else if (rightConsumerIndex >= 0){

                    if (expression[rightConsumerIndex].priority >= expression[i].priority){

                        expression[i].left = new Operation.Operand(expression[rightConsumerIndex]);

                    }
                    else{

                        expression[i].right = new Operation.Operand(operands.Pop());

                    }

                    rightConsumerIndex = -1;

                    if (operands.Count == 0){
                        
                        if (debug){
                            
                            Console.WriteLine("No more operands");

                        }

                        continue;

                    }

                }

                expression[i].right = new Operation.Operand(operands.Pop());

            }
            
            for (int i = operatorsToRemove.Count - 1 ; i >= 0 ; i--){
                
                expression.RemoveAt(operatorsToRemove[i]);

            }

            // Locate the expression that is able to reference all child nodes
            for (int i = 0 ; i < expression.Count ; i++){

                if (debug){

                    Console.WriteLine(expression[i].ToString());

                    Console.WriteLine($"Comparing Count: {expression.Count} with CountedRefs: {expression[i].CountOperandReferences()}\n------------------------------------");

                }

                if (expression[i].CountOperandReferences() == expression.Count){

                    entryIndex = i;

                    break;

                }

            }

        }

        private int[] ProfileOperators(ref string[] components){

            expression = new List<Operation>();

            List<int> operationIndexBuffer = new List<int>();

            for (int i = 0 ; i < components.Length ; i++){

                if (!double.TryParse(components[i], out _)){ // If parsing fails, the value is an operator

                    operationIndexBuffer.Add(i);

                    expression.Add(
                        new Operation(components[i])
                    );

                }

            }

            return operationIndexBuffer.ToArray();

        }

        /// <summary>
        /// Calculate the value from the parsed expression
        /// </summary>
        /// <returns>The value of the expression</returns>
        public double Calculate(){

            if (debug){

                Console.WriteLine("\n\n\nCalculating:\n");

            }

            double value = expression[entryIndex].Calculate();

            if (debug){

                Console.WriteLine(
                    $"Entry at: {entryIndex}\nValue: {value}"
                );

            }

            return value;

        }

    }

    class Operation{

        public readonly byte priority;

        public Operand left;
        public Operand right;

        public readonly OperatorInfo operandData;

        private bool hasLeft
        {
            get{
                return operandData == OperatorInfo.LEFTRIGHT || operandData == OperatorInfo.LEFT;
            }
        }

        private bool hasRight
        {
            get{
                return operandData == OperatorInfo.LEFTRIGHT || operandData == OperatorInfo.RIGHT || operandData == OperatorInfo.BRACKETOPEN;
            }
        }

        private readonly int opcode;

        public static bool debug;

        /*
            operandIdentifier:
                0 - left and right
                1 - left
                2 - right
                3 - symbol
                4 - wrapper
            priority:
                0 - lowest (add, subtract)
                ...
                3 - highest (brackets)
        */

        public Operation(string operation){

            string[] operationComponents = operation.Split('_'); // 0: symbol, 1: priority, 2: operand info

            priority = byte.Parse(operationComponents[1]);

            switch (operationComponents[2]){ // Operand info

                case "0":
                    operandData = OperatorInfo.LEFTRIGHT;
                    break;
                
                case "1":
                    operandData = OperatorInfo.LEFT;
                    break;
                
                case "2":
                    operandData = OperatorInfo.RIGHT;
                    break;

                case "3":
                    operandData = OperatorInfo.FUNCTION;
                    break;
                
                case "4":
                    operandData = OperatorInfo.BRACKETOPEN;
                    break;
                
                case "5":
                    operandData = OperatorInfo.BRACKETCLOSE;
                    return;
                
                case "6":
                    operandData = OperatorInfo.CONSTANT;
                    break;

                default:
                    throw new ArgumentException("Invalid operational quantities");

            }

            switch (operationComponents[0]){
            
                case "+":
                    opcode = 0x00;
                    break;
            
                case "-":
                    opcode = 0x01;
                    break;
            
                case "-inv":
                    opcode = 0x02;
                    break;
            
                case "*":
                    opcode = 0x03;
                    break;
            
                case "/":
                    opcode = 0x04;
                    break;
                
                // Close bracket not implemented as it is simply a marker
                case "(":
                    opcode = 0x05;
                    break;
                
                case "^":
                    opcode = 0x06;
                    break;

                default:
                    throw new ArgumentException("The given operation is not implemented");

            }

        }

        public int CountOperandReferences(){

            int references = 1; // Start with 1 to account for itself

            if (hasLeft && !left.isConstant){

                references += left.childReferenceCount;

            }

            if (hasRight && !right.isConstant){

                references += right.childReferenceCount;

            }

            return references;

        }

        public double Calculate(){

            double leftValue = left.value;

            double rightValue = right.value;

            if (debug){
                
                Console.WriteLine(this.ToString());

                Console.WriteLine($"Using Opcode: {opcode}\nLeft: {leftValue}\nRight: {rightValue}\n---------------------------------------------------------------------");

            }

            switch (opcode){

                case 0x00: // Add
                    return leftValue + rightValue;

                case 0x01: // Subtract
                    return leftValue - rightValue;

                case 0x02: // Subtract Inverted
                    return rightValue - leftValue;

                case 0x03: // Multiply
                    return leftValue * rightValue;

                case 0x04: // Divide
                    return leftValue / rightValue;
                
                case 0x05: // Open bracket
                    return rightValue;
                
                case 0x06:
                    return Math.Pow(leftValue, rightValue);

                default:
                    throw new ArgumentException("The given opcode has not yet been implemented.");

            }

        }

        public override string ToString(){

            return $"Priority: {priority}\nHas Left: {hasLeft}\nHas Right: {hasRight}\nOpcode: {opcode}\nLeft Is Constant: {left.isConstant}\nRight Is Constant: {right.isConstant}";

        }

        public enum OperatorInfo{
            LEFTRIGHT,
            LEFT,
            RIGHT,
            FUNCTION,
            BRACKETOPEN,
            BRACKETCLOSE,
            CONSTANT
        }

        public readonly struct Operand{
            private double constantValue { get; }
            private Operation referenceValue { get; }
            public double value
            {
                get{
                    if (referenceValue == null)
                        return constantValue;
                    return referenceValue.Calculate();
                }
            }
            public bool isConstant{
                get{
                    return referenceValue == null;
                }
            }
            public int childReferenceCount
            {
                get{
                    if (!isConstant)
                        return referenceValue.CountOperandReferences();
                    return 0; // 1 instead of 0 as the operand itself needs to be counted for
                }
            }

            public Operand(string constant) : this() => constantValue = (double)double.Parse(constant);

            public Operand(Operation operationRef) : this() => referenceValue = operationRef;
        }

    }

}