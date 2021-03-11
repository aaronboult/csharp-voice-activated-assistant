using System;
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
        /// Perform a test on the parser
        /// </summary>
        /// <param name="expression">The expression to parse</param>
        /// <param name="expected">The expected result</param>
        public static bool __PerformTest__(string expression, string expected, bool debug = false){

            Tree testTree = new Tree(expression, debug);

            string result = testTree.Calculate().ToString();

            Console.WriteLine($"Expression {expression} Passed: {result == expected}");

            return result == expected;

        }

        /// <summary>
        /// Construct a new Tree object
        /// </summary>
        /// <param name="equation">The equation to parse</param>
        /// <param name="debug">Whether or not to display debug information</param>
        public Tree(string equation, bool debug = false){

            this.debug = debug;

            Operation.debug = debug;

            if (debug){

                Console.WriteLine($"Generating tree with equation: '{equation}'");

            }

            string[] components = equation.Split(' ');

            if (components.Length == 1){

                expression = new List<Operation>();

                expression.Add(
                    new Operation("+_0_0")
                );

                expression[0].operandOne = new Operation.Operand(components[0]);

                expression[0].operandTwo = new Operation.Operand("0");

                return;

            }

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

            bool settingOperandOne = true;

            int operandTwoConsumerIndex = -1; // -1 denotes there is no set operandTwoConsumer

            int lastSingleSidedOperatorIndex = -1;

            if (debug){

                Console.WriteLine("Generation beginning:");

            }

            for (int i = 0 ; i < expression.Count ; i++){

                if (expression[i].operandData == Operation.OperatorInfo.OPERANDDIVIDER){

                    if (lastSingleSidedOperatorIndex != -1){

                        if (expression.Count - 1 > i){

                            if (expression[i + 1].priority < expression[lastSingleSidedOperatorIndex].priority){

                                expression[i + 1].operandOne = new Operation.Operand(expression[lastSingleSidedOperatorIndex]);

                                expression[lastSingleSidedOperatorIndex].operandTwo = new Operation.Operand(operands.Pop());

                                settingOperandOne = false;

                            }
                            else{

                                expression[lastSingleSidedOperatorIndex].operandTwo = new Operation.Operand(expression[i + 1]);

                            }

                        }
                        else{
                            
                            expression[lastSingleSidedOperatorIndex].operandTwo = new Operation.Operand(operands.Pop());
                            
                        }

                    }
                    else if (operandTwoConsumerIndex >= 0){

                        settingOperandOne = false;

                    }

                    operatorsToRemove.Add(i);

                    continue;

                }

                if (expression[i].operandData == Operation.OperatorInfo.BRACKETOPEN){

                    bracketQueue.Push((i, operandTwoConsumerIndex));

                    operandTwoConsumerIndex = -1;

                    settingOperandOne = true;

                    if (expression.Count - 1 > i){

                        expression[bracketQueue.Peek().Item1].operandTwo = new Operation.Operand(expression[i + 1]);

                    }

                    continue;

                }
                else if (expression[i].operandData == Operation.OperatorInfo.BRACKETCLOSE){

                    (int, int) previousState = bracketQueue.Pop();

                    operandTwoConsumerIndex = previousState.Item2;

                    operatorsToRemove.Add(i);

                    if (expression.Count - 1 > i){

                        expression[i + 1].operandOne = new Operation.Operand(expression[operandTwoConsumerIndex]);

                        if (bracketQueue.Count > 0){

                            expression[bracketQueue.Peek().Item1].operandTwo = new Operation.Operand(expression[i + 1]);

                        }

                    }

                    settingOperandOne = false;

                    continue;

                }

                if (expression[i].operandData == Operation.OperatorInfo.RIGHTRIGHT){

                    lastSingleSidedOperatorIndex = i;
                    
                    if (expression.Count - 1 > i){

                        if (expression[i + 1].operandData != Operation.OperatorInfo.OPERANDDIVIDER){

                            settingOperandOne = false;

                        }

                    }

                }

                if (settingOperandOne){

                    expression[i].operandOne = new Operation.Operand(operands.Pop());

                }

                settingOperandOne = true;

                if (expression.Count - 1 > i){

                    if (expression[i].priority >= expression[i + 1].priority){ // Consume as many operands as needed; assign self to next's operandOne

                        if (operandTwoConsumerIndex < 0 || expression[i].priority == expression[i + 1].priority){

                            expression[i + 1].operandOne = new Operation.Operand(expression[i]);

                            if (operandTwoConsumerIndex >= 0){ // If there is a operandTwoConsumer set, update where it's operandTwo reference points to

                                expression[operandTwoConsumerIndex].operandTwo = new Operation.Operand(expression[i + 1]);

                            }

                        }
                        else{

                            expression[i + 1].operandOne = new Operation.Operand(expression[operandTwoConsumerIndex]);

                            operandTwoConsumerIndex = -1;

                        }

                        settingOperandOne = false;

                    }
                    else{

                        if (expression[i].operandData == Operation.OperatorInfo.RIGHTRIGHT){

                            if (expression[i + 1].operandData == Operation.OperatorInfo.BRACKETOPEN){

                                expression[i].operandOne = new Operation.Operand(expression[i + 1]);

                                operandTwoConsumerIndex = i;

                            }

                            continue;

                        }
                        
                        if (expression[i + 1].operandData != Operation.OperatorInfo.BRACKETCLOSE){

                            operandTwoConsumerIndex = i;

                            if (expression[i + 1].operandData != Operation.OperatorInfo.OPERANDDIVIDER){
                            
                                expression[i].operandTwo = new Operation.Operand(expression[i + 1]);

                                continue;

                            }

                        }

                    }

                }
                else if (operandTwoConsumerIndex >= 0){

                    if (expression[operandTwoConsumerIndex].priority >= expression[i].priority){

                        expression[i].operandOne = new Operation.Operand(expression[operandTwoConsumerIndex]);

                    }

                    operandTwoConsumerIndex = -1;

                    if (operands.Count == 0){
                        
                        if (debug){
                            
                            Console.WriteLine("No more operands");

                        }

                        continue;

                    }

                }

                expression[i].operandTwo = new Operation.Operand(operands.Pop());

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

                    if (components[i].Split('_').Length == 2){ // Only contains symbol and operand type

                        continue;

                    }

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

        public Operand operandOne;
        public Operand operandTwo;

        public readonly OperatorInfo operandData;

        private readonly int opcode;

        public static bool debug;

        /*
            operandIdentifier:
                0 - operandOne and operandTwo
                1 - operandOne lwdr
                2 - operandTwo operandTwo
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

            if (debug){

                Console.WriteLine(operationComponents[2]);

            }

            switch (operationComponents[2]){ // Operand info

                case "0":
                    operandData = OperatorInfo.LEFTRIGHT;
                    break;
                
                case "1":
                    operandData = OperatorInfo.LEFTLEFT;
                    break;
                
                case "2":
                    operandData = OperatorInfo.RIGHTRIGHT;
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
                    operandData = OperatorInfo.OPERANDDIVIDER;
                    return;

                case "7":
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
                
                case "rt":
                    opcode = 0x07;
                    break;
                
                case "log":
                    opcode = 0x08;
                    break;
                
                default:
                    throw new ArgumentException("The given operation is not implemented");

            }

        }

        public int CountOperandReferences(){

            if (operandData == OperatorInfo.BRACKETCLOSE){

                return 0;

            }

            return 1 + operandOne.childReferenceCount + operandTwo.childReferenceCount;

        }

        public double Calculate(){

            double valueOne = operandOne.value;

            double valueTwo = operandTwo.value;

            if (debug){
                
                Console.WriteLine(this.ToString());

                Console.WriteLine($"Using Opcode: {opcode}\nLeft: {valueOne}\nRight: {valueTwo}\n---------------------------------------------------------------------");

            }

            switch (opcode){

                case 0x00: // Add
                    return valueOne + valueTwo;

                case 0x01: // Subtract
                    return valueOne - valueTwo;

                case 0x02: // Subtract Inverted
                    return valueTwo - valueOne;

                case 0x03: // Multiply
                    return valueOne * valueTwo;

                case 0x04: // Divide
                    return valueOne / valueTwo;
                
                case 0x05: // Open bracket
                    return valueTwo;
                
                case 0x06:
                    return Math.Pow(valueOne, valueTwo);
                
                case 0x07:
                    return Math.Pow(valueTwo, 1 / valueOne);
                
                case 0x08:
                    return Math.Log(valueTwo, valueOne);

                default:
                    throw new ArgumentException("The given opcode has not yet been implemented.");

            }

        }

        public override string ToString(){

            return $"Priority: {priority}\nOperand Info: {operandData}\nOpcode: {opcode}\nLeft Is Constant: {operandOne.isConstant}\nRight Is Constant: {operandTwo.isConstant}";

        }

        public enum OperatorInfo{
            LEFTRIGHT,
            LEFTLEFT,
            RIGHTRIGHT,
            FUNCTION,
            BRACKETOPEN,
            BRACKETCLOSE,
            OPERANDDIVIDER,
            CONSTANT
        }

        public readonly struct Operand{
            private double constantValue { get; }
            private Operation referenceValue { get; }
            public double value
            {
                get{
                    return referenceValue == null ? constantValue : referenceValue.Calculate();
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
                    return isConstant ? 0 : referenceValue.CountOperandReferences();
                }
            }

            public Operand(string constant) : this(){

                double value;

                if (!double.TryParse(constant, out value)){

                    switch (constant.Split('_')[0]){ // Assign constants

                        case "e":
                            value = Math.E;
                            break;
                        
                        case "pi":
                            value = Math.PI;
                            break;
                        
                        case "c":
                            value = 300_000_000;
                            break;

                    }

                }

                constantValue = value;

            }

            public Operand(Operation operationRef) : this() => referenceValue = operationRef;
        }

    }

}