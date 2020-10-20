using System;
using System.Linq;
using System.Collections.Generic;

namespace MathAbstractions{

    public class Tree{

        Branch entry;

        bool debug = false;

        public static void __Test__(){

            PerformTest("5 + 5", 10);

            PerformTest("3 * 3 - 2 * 4", 1);

            PerformTest("9 - 8 * 7 + 6 / 2", -44);

            PerformTest("2 * 3 * 4 * 5 * 6 * 7", 5040);

        }

        static void PerformTest(string expression, float expected){

            Tree testTree = new Tree(expression, true); // Expect 9

            Console.WriteLine($"Expression {expression} Passed: {(testTree.Calculate() == expected)}\n\n");

        }

        public Tree(string equation, bool debug = false){

            this.debug = debug;

            List<string> components = equation.Split(" "[0]).ToList(); // Equation should be in the format: [operand, operator, operand, operator, operand...]

            Branch leftNode = null;

            Branch currentBranch = null;

            int positionOffset = 0;

            for (int i = 0 ; i < components.Count - 1 ; i+=2){

                if (components[i + 1] == "*" || components[i + 1] == "/" || components.Count - 3 <= i){

                    currentBranch = new Branch(components[i + 1], components[i + 2]);

                }
                else if (components[i + 3] == "*" || components[i + 3] == "/"){

                    Branch tempBranch = new Branch(components[i + 3], components[i + 2], components[i + 4]);

                    currentBranch = new Branch(components[i + 1], tempBranch);

                    positionOffset = 2;

                }
                else{
                    
                    currentBranch = new Branch(components[i + 1], components[i + 2]);

                }

                if (leftNode == null){

                    currentBranch.left = float.Parse(components[i]);

                }
                else{
                    
                    currentBranch.leftPointer = leftNode;

                }

                leftNode = currentBranch;
                
                i += positionOffset;

                positionOffset = 0;

            }

            this.entry = currentBranch;

        }

        public float Calculate(){

            float value = this.entry.Calculate(this.debug);

            Console.WriteLine(
                $"{value}"
            );

            return value;

        }

    }

    class Branch{

        public float value;
        public bool calculated = false;

        public float left;

        Branch _leftPointer = null;
        public Branch leftPointer{

            get{
                return _leftPointer;
            }

            set{
                _leftPointer = _leftPointer == null ? value : _leftPointer;
            }

        }

        public float right;
        
        Branch _rightPointer = null;
        public Branch rightPointer{

            get{
                return _rightPointer;
            }

            set{
                _rightPointer = _rightPointer == null ? value : _rightPointer;
            }

        }

        Operation operation;
        
        bool inverse = false;

        enum Operation{
            ADD, SUBTRACT,
            MULTIPLY, DIVIDE
        }

        public Branch(string operation, string left, string right) : this(operation, right){

            this.left = float.Parse(left);

        }

        public Branch(string operation, string right){

            this.operation = GetOperation(operation);

            this.right = float.Parse(right);

        }

        public Branch(string operation, Branch right){

            this.operation = GetOperation(operation);

            this.rightPointer = right;

        }

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

            }

            return Operation.ADD;

        }

        public float Calculate(bool debug = false){

            if (this.calculated){

                return this.value;

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