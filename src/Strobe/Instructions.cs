using System;
using System.Collections.Generic;
using System.Text;

namespace Strobe
{
    public partial class CodeGenerator
    {

        void GenerateInstruction(Instruction i, Function func, Args args, Function old)
        {
            switch (i.Func.Function)
            {
                /*
                 * Handle Blocks
                 */
                case "_ !block":
                    // Handle the block
                    Generate(new Function
                    {
                        // Keep the name (because of variables).
                        Name = func.Name,
                        // Copy the instructions
                        Instructions = ((IBlock)i).List,
                        // Return nothing
                        Ret = new Return(),
                        // Take no arguments
                        Arguments = new Args
                        {
                            // Because
                            Arguments = new List<Variable>()
                        }
                    }, null, func);
                    break;
                /*
                 * Define new variable from constant
                 */
                case "new":
                    // Check if it has valid arguments
                    if (i.Func.Arguments.Arguments.Count == 1 && i.Func.Arguments.Arguments[0].isConst)
                    {
                        byte[] x;
                        // Check if it's a number or string
                        if (i.Func.Arguments.Arguments[0].isNum)
                        {
                            // Number
                            x = BitConverter.GetBytes(int.Parse(i.Func.Arguments.Arguments[0].Name));
                        }
                        else
                        {
                            // String
                            x = Encoding.ASCII.GetBytes(i.Func.Arguments.Arguments[0].Name);
                        }
                        // Define the variable
                        int var = AddVariable(x);

                        // Assign the value to the variable
                        if (i.Op?.Type == "=")
                        {
                            // Check if it's valid
                            if (i.Var?.isConst == false)
                            {
                                // Is it direct? (starts with x)
                                if (i.Var.Name.ToLower().StartsWith("x"))
                                    // Directly move
                                    Move(int.Parse(i.Var.Name.ToLower().TrimStart('x')), var);
                                else
                                {
                                    // Define if not already defined
                                    if (!Vars.ContainsKey(func.Name + i.Var.Name))
                                    {
                                        Vars.Add(func.Name + i.Var.Name, var);
                                    }
                                    else
                                    {
                                        // Change if already defined
                                        Vars[func.Name + i.Var.Name] = var;
                                    }
                                }
                            }
                        }
                        // Throw an exception
                    }
                    else throw new Exception("Invalid arguments in `new`.");
                    break;
                /*
                 * Multiply two numbers
                 */
                case "proc_Mul":
                    // Check for valid size of arguments.
                    if (i.Func.Arguments.Arguments.Count == 2)
                    {
                        Mul(GetArgument(i, func, 0), GetArgument(i, func, 1));
                    }
                    else throw new Exception("Invalid arguments in `proc_Mul`.");
                    break;
                /*
                 * Divide two numbers
                 */
                case "proc_Div":
                    // Check for valid size of arguments.
                    if (i.Func.Arguments.Arguments.Count == 2)
                    {
                        Div(GetArgument(i, func, 0), GetArgument(i, func, 1));
                    }
                    else throw new Exception("Invalid arguments in `proc_Div`.");
                    break;
                /*
                 * Subtract two numbers
                 */
                case "proc_Sub":
                    // Check for valid size of arguments.
                    if (i.Func.Arguments.Arguments.Count == 2)
                    {
                        Sub(GetArgument(i, func, 0), GetArgument(i,func,1));
                    }
                    else throw new Exception("Invalid arguments in `proc_Sub`.");
                    break;
                /*
                 * Add two numbers
                 */
                case "proc_Add":
                    // Check for valid size of arguments.
                    if (i.Func.Arguments.Arguments.Count == 2)
                    {
                        Add(GetArgument(i,func,0), GetArgument(i,func,1));
                    }
                    else
                        // Thow an exception
                        throw new Exception("Invalid arguments in `proc_Add`.");
                    break;
                    /*
                     * Create a label
                     */
                case "label":
                    if (i.Func.Arguments.Arguments.Count == 1 && !i.Func.Arguments.Arguments[0].isConst)
                    {
                        int var = AddVariable(BitConverter.GetBytes(++LabelID));
                        Label(var);
                        if (i.Func.Arguments.Arguments[0].Name.ToLower().StartsWith("x"))
                            // Directly move
                            Move(int.Parse(i.Func.Arguments.Arguments[0].Name.ToLower().TrimStart('x')), var);
                        else
                        {
                            // Define if not already defined
                            if (!Vars.ContainsKey(func.Name + i.Func.Arguments.Arguments[0].Name))
                            {
                                Vars.Add(func.Name + i.Func.Arguments.Arguments[0].Name, var);
                            }
                            else
                            {
                                // Change if already defined
                                Vars[func.Name + i.Func.Arguments.Arguments[0].Name] = var;
                            }
                        }
                    }
                    else throw new Exception("Invalid arguments in `label`.");
                    break;
                case "goto":
                    if (i.Func.Arguments.Arguments.Count == 2 && !i.Func.Arguments.Arguments[0].isConst && !i.Func.Arguments.Arguments[1].isConst)
                    {
                        // 30 Lines less xD
                        Goto(GetArgument(i, func, 0), GetArgument(i, func, 1));
                    }
                    else throw new Exception("Invalid arguments in `goto`.");
                    break;
                /*
                 * Clear Value
                 */
                case "clear":
                    // Check for the arguments count.
                    if (i.Func.Arguments.Arguments.Count == 1)
                    {
                        // Clear the variable.
                        ClearVar(GetArgument(i,func,0));
                    }
                    else throw new Exception("Invalid arguments in `clear`.");
                    break;
                    /*
                     * Move Functions
                     */ 
                case "address":
                    if (i.Func.Arguments.Arguments.Count == 2)
                    {
                        Move(GetArgument(i,func,0),GetArgument(i,func,1));
                    }
                    break;
                case "content":
                    if (i.Func.Arguments.Arguments.Count == 2)
                    {
                        MoveC(GetArgument(i, func, 0), GetArgument(i, func, 1));
                    }
                    break;
                    /*
                     * Compiler Functions
                     */ 
                case "set_label":
                    if (i.Func.Arguments.Arguments.Count == 1 && i.Func.Arguments.Arguments[0].isNum)
                    {
                        CurrentVar = int.Parse(16 + i.Func.Arguments.Arguments[0].Name);
                    }
                    else throw new Exception("Invalid arguments in `set_label`.");
                    break;
                case "set_variable":
                    if (i.Func.Arguments.Arguments.Count == 1 && i.Func.Arguments.Arguments[0].isNum)
                    {
                        LabelID = int.Parse(i.Func.Arguments.Arguments[0].Name);
                    }
                    else throw new Exception("Invalid arguments `in set_variable`.");
                    break;
                /*
                 * Get value
                 */
                case "get":
                    // Check for the arguments count
                    if (i.Func.Arguments.Arguments.Count == 1)
                    {
                        int var = GetArgument(i,func,0);

                        // Check if sould assign
                        if (i.Op?.Type == "=")
                        {
                            // Check if the variable is valid
                            if (i.Var?.isConst == false)
                            {

                                // If it starts with "x", directly put into the variable
                                if (i.Var.Name.ToLower().StartsWith("x"))
                                    // Move.
                                    Move(int.Parse(i.Var.Name.ToLower().TrimStart('x')), var);
                                else
                                {
                                    // If the variable isn't defined, define it.
                                    if (!Vars.ContainsKey(func.Name + i.Var.Name))
                                    {
                                        Vars.Add(func.Name + i.Var.Name, var);
                                    }
                                    else
                                    {
                                        // Change the variable
                                        Vars[func.Name + i.Var.Name] = var;
                                    }
                                }
                            }
                        }
                    }
                    else
                        // Throw the exception
                        throw new Exception("Incorrect amount of arguments in `get`.");
                    break;
                /*
                 * The Is function.
                 */
                case "is":
                    // Default size is 4
                    int mSize = 4;
                    // If there is a second argument
                    if (i.Func.Arguments.Arguments.Count == 2)
                    {
                        // And it's a number
                        if (i.Func.Arguments.Arguments[1].isNum)
                            // Set the size to that number
                            mSize = int.Parse(i.Func.Arguments.Arguments[1].Name);
                        else
                            // If not, thow an exception
                            throw new Exception("Invalid argument in `is`.");
                    }
                    // Select the old variable
                    int oldVar = 0;
                    // Create the new variable
                    int newVar = AddVariable(new byte[mSize]);
                    // Get your sources
                    if (i.Func.Arguments.Arguments[0].isConst == false)
                    {
                        // Check if it is an address
                        if (i.Func.Arguments.Arguments[0].Name.ToLower().StartsWith("x"))
                            // Set the oldVar address to that address.
                            oldVar = int.Parse(i.Func.Arguments.Arguments[0].Name.ToLower().TrimStart('x'));
                        else
                            // Find a predefined variable.
                            oldVar = Vars[func.Name + i.Func.Arguments.Arguments[0].Name];
                    }
                    else
                        throw new Exception("Invalid argument in `is`.");
                    // Where to assign
                    if (i.Op?.Type == "=")
                    {
                        // Check if the variable is valid
                        if (i.Var?.isConst == false)
                        {

                            // If it starts with "x", directly put into the variable
                            if (i.Var.Name.ToLower().StartsWith("x"))
                                // Move.
                                newVar = int.Parse(i.Var.Name.ToLower().TrimStart('x'));
                            else
                            {
                                // If the variable isn't defined, define it.
                                if (!Vars.ContainsKey(func.Name + i.Var.Name))
                                {
                                    Vars.Add(func.Name + i.Var.Name, newVar);
                                }
                                else
                                {
                                    // Change the variable
                                    Vars[func.Name + i.Var.Name] = newVar;
                                }
                            }
                        }
                    }
                    MoveC(newVar, oldVar);
                    break;
                /*
                 * Interrupt
                 */
                case "int":
                    // Check if the arguments are correct & interrupt
                    if (i.Func.Arguments.Arguments.Count == 1 && i.Func.Arguments.Arguments[0].isNum)
                    {
                        int x = int.Parse(i.Func.Arguments.Arguments[0].Name);
                        // Interrupt
                        Interrupt((byte)x);
                    }
                    else
                        // Thow the exception
                        throw new Exception("Interrupt can only accept bytes as arguments.");
                    break;
                /*
                 * Execute Function
                 */
                default:
                    // Find the function
                    Function f = find(i.Func.Namespace, i.Func.Function);
                    // Set the function name to something unique to the namespace
                    f.Name = i.Func.Namespace + " " + i.Func.Function + " ";
                    // Generate the function, and return.
                    Generate(f, i.Func.Arguments, func);
                    // Return if it's a variable
                    if (f.Ret?.Type == TokenType.Variable)
                    {
                        int var;
                        // Check if should return address.
                        if (f.Ret.Value.ToLower().StartsWith("x"))
                            // Get direct address.
                            var = int.Parse(f.Ret.Value.ToLower().TrimStart('x'));
                        else
                            // Get variable.
                            var = Vars[f.Name + f.Ret.Value];
                        // Check if should assign
                        if (i.Op?.Type == "=")
                        {
                            if (i.Var?.isConst == false)
                            {
                                // Check if it's a direct address
                                if (i.Var.Name.ToLower().StartsWith("x"))
                                    // Move the data**
                                    MoveC(int.Parse(i.Var.Name.ToLower().TrimStart('x')), var);
                                else
                                {
                                    // Define the variable if it's not already defined
                                    if (!Vars.ContainsKey(func.Name + i.Var.Name))
                                    {
                                        // Define Variable
                                        Vars.Add(func.Name + i.Var.Name, var);
                                    }
                                    else
                                    {
                                        // Change Variable
                                        Vars[func.Name + i.Var.Name] = var;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Make sure that it's an invalid return and not a void
                        if (f.Ret?.Type == TokenType.Number
                            || f.Ret.Type == TokenType.String)
                            throw new Exception("Functions can only return variables!");
                    }
                    // Collect the garbadge (if not in block)
                    if ((old != null) && (old.Name == func.Name))
                        CollectGarbadge(f.Name);
                    break;
            }
        }
    }
}
