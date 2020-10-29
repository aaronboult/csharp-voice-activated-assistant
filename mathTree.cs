using System;
using System.Linq;
using System.Collections.Generic;

namespace MathsAbstractions{

    /// <summary>
    /// A collection of ordered components in an expression based on BIDMAS
    /// </summary>
    public class Tree{

        Branch entry;

        bool debug = false;

        /// <summary>
        /// Perform tests to check whether the parser is working properly
        /// </summary>
        public static void __Test__(){

            __PerformTest__("5 + 5", 10);

            __PerformTest__("3 * 3 - 2 * 4", 1);

            __PerformTest__("9 - 8 * 7 + 6 / 2", -44);

            __PerformTest__("2 * 3 * 4 * 5 * 6 * 7", 5040);

            __PerformTest__("-6 + 3", -3);

            __PerformTest__("3 -inv 6", 3);

            __PerformTest__("2 * ( 3 + 5 )", 16);

            __PerformTest__("3 * ( 2 * ( 3 + 5 ) )", 48);

            __PerformTest__("2 * ( 6 / ( 1 + ( 4 / 2 ) ) )", 4);

            __PerformTest__("3 * ( 4 / 2 ) - 1", 5);

            __PerformTest__("2 * ( 3 * ( 1 + 1 ) - 2 )", 8);

        }

        /// <summary>
        /// Perform a test on the parser
        /// </summary>
        /// <param name="expression">The expression to parse</param>
        /// <param name="expected">The expected result</param>
        static void __PerformTest__(string expression, float expected){

            Tree testTree = new Tree(expression, true); // Expect 9

            Console.WriteLine($"Expression {expression} Passed: {(testTree.Calculate() == expected)}\n\n");

        }

        /// <summary>
        /// Construct a new Tree object
        /// </summary>
        /// <param name="equation">The equation to parse</param>
        /// <param name="debug">Whether or not to display debug information</param>
        public Tree(string equation, bool debug = false){

            this.debug = debug;

            List<string> components = equation.Split(" "[0]).ToList(); // Equation should be in the format: [operand, operator, operand, operator, operand...]

            Branch leftBranch = null;

            Branch currentBranch = null;

            List<(Branch, string, string)> bracketBuffer = new List<(Branch, string, string)>();

            List<string> higherOperations = new List<string>{
                "*", "/"
            };

            (string, string) bracketLeftOperation = (null, null);

            int positionOffset = 0;

            for (int i = 0 ; i < components.Count - 1 ; i+=2){

                if (components[i + 1] == ")"){

                    currentBranch = leftBranch;

                    leftBranch = bracketBuffer[bracketBuffer.Count - 1].Item1;

                    (_, string operation, string operand) = bracketBuffer[bracketBuffer.Count - 1];

                    bracketLeftOperation = (operation, operand);

                    bracketBuffer.RemoveAt(bracketBuffer.Count - 1);

                    i++;

                    positionOffset = -2;

                }
                else{

                    if (components[i] == "("){

                        (Branch, string, string) bracket = (leftBranch, null, null); // Left branch, operation, operand

                        if (leftBranch == null && i - 2 >= 0){

                            bracket = (null, components[i - 1], components[i - 2]);

                        }

                        bracketBuffer.Add(bracket);

                        leftBranch = null;

                        i++;

                    }

                    if (components.Count - 3 >= i){

                        if (components[i + 2] == "("){

                            continue;

                        }
                        
                    }

                    if (higherOperations.Contains(components[i + 1]) || components.Count - 3 <= i){ // If the operator is either a higher order operator or the end has been reached

                        currentBranch = new Branch(components[i + 1], components[i + 2]);

                    }
                    else if (higherOperations.Contains(components[i + 3])){ // If the next operation is to be executed before the current

                        Branch tempBranch = new Branch(components[i + 3], components[i + 2], components[i + 4]); // Create a branch encasing the next operation

                        currentBranch = new Branch(components[i + 1], tempBranch);

                        positionOffset = 2; // Skip the operation just encased

                    }
                    else{
                        
                        currentBranch = new Branch(components[i + 1], components[i + 2]);

                    }

                }

                if (leftBranch == null){

                    if (bracketLeftOperation.Item2 == null){

                        currentBranch.left = float.Parse(components[i]); // Current branch acts as the initial starting point

                    }
                    else{

                        leftBranch = new Branch(bracketLeftOperation.Item1, currentBranch);

                        leftBranch.left = float.Parse(bracketLeftOperation.Item2);

                        currentBranch = leftBranch;

                        bracketLeftOperation = (null, null);

                    }

                }
                else{
                    
                    currentBranch.leftPointer = leftBranch; // Point the input of the left side of the current expression to the output of the previous expression

                }

                leftBranch = currentBranch;
                
                i += positionOffset;

                positionOffset = 0;

            }

            this.entry = currentBranch;

        }

        /// <summary>
        /// Calculate the value from the parsed expression
        /// </summary>
        /// <returns>The value of the expression</returns>
        public float Calculate(){

            float value = this.entry.Calculate(this.debug);

            Console.WriteLine(
                $"{value}"
            );

            return value;

        }

    }

    /// <summary>
    /// A single branch expression that applies a certain operation to a left and right value
    /// </summary>
    class Branch{

        public float value;
        public bool calculated = false; // Used to avoid unnecessary recalculations

        public float left;

        Branch _leftPointer = null;
        public Branch leftPointer{

            get{
                return _leftPointer;
            }

            set{
                _leftPointer = _leftPointer == null ? value : _leftPointer; // Preent the pointer being overwritten
            }

        }

        public float right;
        
        Branch _rightPointer = null;
        public Branch rightPointer{

            get{
                return _rightPointer;
            }

            set{
                _rightPointer = _rightPointer == null ? value : _rightPointer; // Prevent the pointer being overwritten
            }

        }

        Operation operation;
        
        bool inverse = false; // Whether the 'left' expression should be on the left (false) or on the right (true)

        /// <summary>
        /// Determines the operation to be carried out
        /// </summary>
        enum Operation{
            ADD, SUBTRACT,
            MULTIPLY, DIVIDE
        }

        /// <summary>
        /// Construct a new Branch object
        /// </summary>
        /// <param name="operation">The operation to be carried out on the two inputs</param>
        /// <param name="left">The left side constant of the expression</param>
        /// <param name="right">The right side constant of the expression</param>
        public Branch(string operation, string left, string right) : this(operation, right){

            this.left = float.Parse(left);

        }

        /// <summary>
        /// Constructs a new Branch object
        /// </summary>
        /// <param name="operation">The operation to be carried out on the two inputs</param>
        /// <param name="right">The right side constant of the expression</param>
        public Branch(string operation, string right){

            this.operation = GetOperation(operation);

            this.right = float.Parse(right);

        }

        /// <summary>
        /// Constructs a new Branch object
        /// </summary>
        /// <param name="operation">The operation to be carried out on the two inputs</param>
        /// <param name="right">An instance of a Branch with both left and right sides of the expression set</param>
        public Branch(string operation, Branch right){

            this.operation = GetOperation(operation);

            this.rightPointer = right;

        }

        /// <summary>
        /// Get an operation enumerable from a string
        /// </summary>
        /// <param name="operation">The operation to be carried out</param>
        /// <returns>An operation enumerable representing the operation to be carried out</returns>
        Operation GetOperation(string operation){

            switch (operation){

                case "+":
                    return Operation.ADD;
                
                case "-":
                    return Operation.SUBTRACT;
                
                case "*":
                    return Operation.MULTIPLY;

                case "/":
                    return Operation.DIVIDE;
                
                case "-inv":
                    this.inverse = true;
                    return Operation.SUBTRACT;

            }

            throw new Exception($"Unexpected or unimplemented operation: {operation}");

        }

        /// <summary>
        /// Calculate the value of the expression
        /// </summary>
        /// <param name="debug">Whether or not to show debug information</param>
        /// <returns>The value of the expression calculated</returns>
        public float Calculate(bool debug = false){

            if (this.calculated){

                return this.value; // Avoid recalculating the same value

            }
            
            float leftOperand = this.leftPointer != null 
                ? this.leftPointer.Calculate(debug) : this.left;

            float rightOperand = this.rightPointer != null 
                ? this.rightPointer.Calculate(debug) : this.right;
            
            if (debug){

                Console.WriteLine($"Using {leftOperand} and left operand.");
                
                Console.WriteLine($"Using {rightOperand} and right operand.");

            }

            switch (this.operation){

                case Operation.ADD:
                    this.value = leftOperand + rightOperand;
                    break;
                
                case Operation.SUBTRACT:
                    this.value = this.inverse 
                        ? rightOperand - leftOperand : leftOperand - rightOperand;
                    break;
                
                case Operation.MULTIPLY:
                    this.value = leftOperand * rightOperand;
                    break;
                
                case Operation.DIVIDE:
                    this.value = inverse 
                        ? rightOperand / leftOperand : leftOperand / rightOperand;
                    break;

            }

            this.calculated = true;

            return this.value;

        }

    }

}