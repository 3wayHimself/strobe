﻿using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Strobe
{
	/// <summary>
	/// Code generator.
	/// </summary>
	public class CodeGenerator
	{
        /// <summary>
        /// The label id
        /// </summary>
        int LabelID = 0;

		/// <summary>
		/// The lib folder.
		/// </summary>
		static string libf = Path.GetDirectoryName(
			Assembly.GetExecutingAssembly().Location) + "/lib/";
		
		/// <summary>
		/// The Result
		/// </summary>
		Result Res = new Result();

		/// <summary>
		/// The functions.
		/// </summary>
		Dictionary<string,Function> Functions = new Dictionary<string,Function>();

		/// <summary>
		/// The input.
		/// </summary>
		readonly ParseTree Input;

		/// <summary>
		/// The current variable.
		/// </summary>
		int CurrentVar;

		/// <summary>
		/// The variables.
		/// </summary>
		Dictionary<string,int> Vars = new Dictionary<string, int> ();

		/// <summary>
		/// The output.
		/// </summary>
		List<byte> Output = new List<byte>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Strobe.CodeGenerator"/> class.
		/// </summary>
		/// <param name="Input">Input.</param>
		public CodeGenerator (ParseTree Input)
		{
			// We use 0x2c because of safety.
			CurrentVar = 0x2c;
			this.Input = Input;
			// Generate the code.
			Generate();
		}

		/// <summary>
		/// Generate the code.
		/// </summary>
		void Generate()
		{
			// For include
			foreach (string s in Input.Preprocessor)
				Preprocess (s);

			// Actually import the namespaces & classes
			foreach (Namespace n in Input.Namespaces)
				foreach (Function f in n.Functions)
					Functions.Add (n.Name + f.Name, f);

			// Generate the code, starting with main
			// TODO: Pass arguments instead of null.
			Generate (findMain(),null,null);
		}

		/// <summary>
		/// Preprocess the specified string.
		/// </summary>
		/// <param name="s">String.</param>
		void Preprocess(string s)
		{
			// Hell..

			// Include
			if (s.StartsWith("include")) {
				// Remove the "include" from the string
				s = s.TrimStart("include".ToCharArray());
				// Remove the spaces.
				while(s.StartsWith(" "))
					s = s.TrimStart(" ".ToCharArray());
				// Include from /lib/
				if (s.StartsWith ("<")) {
					while (s.EndsWith (" "))
						s = s.TrimEnd (" ".ToCharArray ());
					// Remove < and >.
					s = s.TrimEnd (">".ToCharArray ());
					s = s.TrimStart ("<".ToCharArray ());

					// Add the lib folder.
					s = libf + s;

					// Compile the file.
					var x = new Parser (new Simplifier (new Lexer (File.ReadAllText (s))
							.get (false).Tokens).get (false).STokens).get (false).Tree;

					// Add the namespaces
					foreach (Namespace y in x.Namespaces) {
						Input.Namespaces.Add (y);
					}

					// Peprocess
					foreach (string g in x.Preprocessor) {
						Preprocess (g);
					}
					// Load from current folder.
				} else if (s.StartsWith("\"")) {
					// Remove "'s
					s = s.TrimEnd ("\"".ToCharArray ());
					s = s.TrimStart ("\"".ToCharArray ());

					// Compile the file
					var x = new Parser (new Simplifier (new Lexer (File.ReadAllText (s))
						.get (false).Tokens).get (false).STokens).get (false).Tree;

					// Add the namespaces
					foreach (Namespace y in x.Namespaces) {
						Input.Namespaces.Add (y);
					}

					// Preprocess
					foreach (string g in x.Preprocessor) {
						Preprocess (g);
					}
				}
			}
		}

		/// <summary>
		/// Adds the variable.
		/// </summary>
		/// <param name="Bytes">Bytes.</param>
		int AddVariable(byte[] Bytes)
		{
			// Get the current variable space and increase it by 1
			int current = ++CurrentVar;

			// Start allocating
			Output.Add(0x0);
			Output.Add(0x4);

			// Variable ID
			foreach (byte add in BitConverter.GetBytes(current))
				Output.Add(add);

			// Variable Size
			Output.Add(0xfe);
			foreach (byte add in BitConverter.GetBytes(Bytes.Length))
				Output.Add(add);

			// End allocating, and start assigning
			Output.Add(0xff);
			Output.Add(0x0);
			Output.Add(0x5);

			// Variable ID
			foreach(byte add in BitConverter.GetBytes(current))
				Output.Add(add);

			// Add the contents
			Output.Add(0xfe);
			foreach(byte add in Bytes)
				Output.Add(add);

			// End assigning and return the current variable
			Output.Add(0xff);
			return current;
		}

		/// <summary>
		/// Interrupt the specified byte.
		/// </summary>
		/// <param name="x">The byte.</param>
		void Interrupt(byte x)
		{
			//  Start Interrupt
			Output.Add (0x0);
			Output.Add (0x6);

			// Interrupt Byte
			Output.Add (x);

			// End Interrupt
			Output.Add (0xff);
		}

        /// <summary>
        /// Add two numbers.
        /// Please... do not touch this method unless you know what you are doing.
        /// </summary>
        /// <param name="x">First Number</param>
        /// <param name="y">Second Number</param>
        void Add(int x, int y)
        {
            // Start Add
            Output.Add(0x0);
            Output.Add(0x0);
            // First number
            foreach (byte add in BitConverter.GetBytes(x))
                Output.Add(add);

            // Second number
            Output.Add(0xfe);
            foreach (byte add in BitConverter.GetBytes(y))
                Output.Add(add);

            // End Add
            Output.Add(0xff);
        }


        /// <summary>
        /// Subtract two numbers.
        /// Please... do not touch this method unless you know what you are doing.
        /// </summary>
        /// <param name="x">First Number</param>
        /// <param name="y">Second Number</param>
        void Sub(int x, int y)
        {
            // Start Subtract
            Output.Add(0x0);
            Output.Add(0x1);
            // First number
            foreach (byte add in BitConverter.GetBytes(x))
                Output.Add(add);

            // Second number
            Output.Add(0xfe);
            foreach (byte add in BitConverter.GetBytes(y))
                Output.Add(add);

            // End Subtract
            Output.Add(0xff);
        }

        /// <summary>
        /// Divide two numbers.
        /// Please... do not touch this method unless you know what you are doing.
        /// </summary>
        /// <param name="x">First Number</param>
        /// <param name="y">Second Number</param>
        void Div(int x, int y)
        {
            // Start Divide
            Output.Add(0x0);
            Output.Add(0x2);
            // First number
            foreach (byte add in BitConverter.GetBytes(x))
                Output.Add(add);

            // Second number
            Output.Add(0xfe);
            foreach (byte add in BitConverter.GetBytes(y))
                Output.Add(add);

            // End Divide
            Output.Add(0xff);
        }

        /// <summary>
        /// Multiply two numbers.
        /// Please... do not touch this method unless you know what you are doing.
        /// </summary>
        /// <param name="x">First Number</param>
        /// <param name="y">Second Number</param>
        void Mul(int x, int y)
        {
            // Start Multiply
            Output.Add(0x0);
            Output.Add(0x3);
            // First number
            foreach (byte add in BitConverter.GetBytes(x))
                Output.Add(add);

            // Second number
            Output.Add(0xfe);
            foreach (byte add in BitConverter.GetBytes(y))
                Output.Add(add);

            // End Multiply
            Output.Add(0xff);
        }

        /// <summary>
        /// Creates a label.
        /// </summary>
        /// <param name="x"></param>
        void Label(int x)
        {
            // Start Label
            Output.Add(0x0);
            Output.Add(0xa);

            // Add the label variable address
            foreach (byte add in BitConverter.GetBytes(x))
                Output.Add(add);

            // End Label
            Output.Add(0xff);
        }

        void Goto(int x, int y)
        {
            // Start Goto
            Output.Add(0x0);
            Output.Add(0xb);

            // Add the label variable address
            foreach (byte add in BitConverter.GetBytes(x))
                Output.Add(add);

            // Add the check variable address
            Output.Add(0xfe);

            foreach (byte add in BitConverter.GetBytes(y))
                Output.Add(add);

            Output.Add(0xff);
        }

        /// <summary>
        /// Move the contents of an address.
        /// Only use when you know what you are doing.
        /// </summary>
        /// <param name="x">To</param>
        /// <param name="y">From</param>
        void MoveC(int x, int y)
        {
            // Start Move
            Output.Add(0x0);
            Output.Add(0x8);

            // Move to
            foreach (byte add in BitConverter.GetBytes(x))
                Output.Add(add);

            // Move from
            Output.Add(0xfe);
            foreach (byte add in BitConverter.GetBytes(y))
                Output.Add(add);

            // End Move
            Output.Add(0xff);
        }

        /// <summary>
        /// Move the specified addresses.
        /// </summary>
        /// <param name="x">To.</param>
        /// <param name="y">From.</param>
        void Move(int x, int y)
		{
            // Start Move
            Output.Add(0x0);
			Output.Add(0x9);

			// Move to
			foreach (byte add in BitConverter.GetBytes(x))
				Output.Add (add);

			// Move from
			Output.Add(0xfe);
			foreach (byte add in BitConverter.GetBytes(y))
				Output.Add (add);

			// End Move
			Output.Add(0xff);
        }

        /// <summary>
        /// Generate from the specified function.
        /// </summary>
        /// <param name="func">Function.</param>
        void Generate(Function func, Args args, Function old)
		{
			/* 
			 * Welome to hell.
			 * Feel free to touch, but you rollback at the end.
			 */
			// Check for valid arguments
			if (func.Arguments.Arguments.Count != args?.Arguments.Count)
				// If it's not main, throw an exception
				if (args != null)
					throw new Exception ("Incorrect number of arguments");

			// Pass the arguments
			for (int i = 0; i < args?.Arguments.Count; i++) {
				// If the arguments are constant, define them.
				if (args.Arguments [i].isConst) {
					byte[] x;

					// Turn them into bytes, if string using ASCII, if number using BitConverter.
					if (args.Arguments [i].isNum) {
						x = BitConverter.GetBytes (int.Parse (args.Arguments [i].Name));
					} else {
						x = Encoding.ASCII.GetBytes (args.Arguments [i].Name);
					}

					// Register the variable.
					int var = AddVariable (x);

					// Check if to use direct (starting with x)
					if (func.Arguments.Arguments [i].Name.ToLower ().StartsWith ("x")) {
						// Move the address
						Move (int.Parse (func.Arguments.Arguments [i].Name.ToLower ().TrimStart ('x')), var);
					} else {
						// Check if it is already defined, if not, define it.
						if (!Vars.ContainsKey (func.Name + func.Arguments.Arguments [i].Name)) {
							// Define it.
							Vars.Add (func.Name + func.Arguments.Arguments [i].Name, var);
						} else {
							// Change the already defined variable.
							Vars [func.Name + func.Arguments.Arguments [i].Name] = var;
						}
					}
					// If the arguments are not constant, move them.
				} else {
					int var;

					// Check if it's direct, if it is, directly get the value
					if (args.Arguments [i].Name.ToLower ().StartsWith ("x"))
						var = int.Parse (args.Arguments [i].Name.ToLower ().TrimStart ('x'));
					else
						// If not, get the variable
						var = Vars[old.Name + args.Arguments [i].Name];

					// Check if should move to variable or address
					if (func.Arguments.Arguments [i].Name.ToLower ().StartsWith ("x")) {
						// Move the value directly
						Move (int.Parse (func.Arguments.Arguments [i].Name.ToLower ().TrimStart ('x')), var);
					} else {
						// Define new variable
						if (!Vars.ContainsKey (func.Name + func.Arguments.Arguments [i].Name)) {
							Vars.Add (func.Name + func.Arguments.Arguments [i].Name, var);
						} else {
							// Change the address
							Vars [func.Name + func.Arguments.Arguments [i].Name] = var;
						}
					}
				}
			}

			// Parse the instructions
			foreach (Instruction i in func.Instructions) {
				switch (i.Func.Function) {
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
                        },null,func);
                        break;
					/*
					 * Define new variable from constant
					 */
				case "new":
						// Check if it has valid arguments
						if (i.Func.Arguments.Arguments.Count == 1 && i.Func.Arguments.Arguments [0].isConst) {
							byte[] x;
							// Check if it's a number or string
							if (i.Func.Arguments.Arguments [0].isNum) {
								// Number
								x = BitConverter.GetBytes(int.Parse (i.Func.Arguments.Arguments [0].Name));
							} else {
								// String
								x  = Encoding.ASCII.GetBytes(i.Func.Arguments.Arguments [0].Name);
							}
							// Define the variable
							int var = AddVariable(x);

							// Assign the value to the variable
							if (i.Op?.Type == "=") {
								// Check if it's valid
								if (i.Var?.isConst == false) {
									// Is it direct? (starts with x)
									if (i.Var.Name.ToLower ().StartsWith ("x"))
										// Directly move
										Move (int.Parse (i.Var.Name.ToLower ().TrimStart ('x')), var);
									else {
										// Define if not already defined
										if (!Vars.ContainsKey (func.Name + i.Var.Name)) {
											Vars.Add (func.Name + i.Var.Name, var);
										} else {
											// Change if already defined
											Vars [func.Name + i.Var.Name] = var;
										}
									}
								}
							}
							// Throw an exception
						} else throw new Exception ("Invalid arguments in `new`.");
					break;
                    /*
                     * Multiply two numbers
                     */
                    case "proc_Mul":
                        // Check for valid size of arguments.
                        if (i.Func.Arguments.Arguments.Count == 2)
                        {
                            int var1, var2;
                            // Check for the arguments var1
                            if (i.Func.Arguments.Arguments[0].Name.ToLower().StartsWith("x"))
                                var1 = int.Parse(i.Func.Arguments.Arguments[0].Name.ToLower().TrimStart('x'));
                            else
                                var1 = Vars[func.Name + i.Func.Arguments.Arguments[0].Name];

                            // Check for the arguments var2
                            if (i.Func.Arguments.Arguments[1].Name.ToLower().StartsWith("x"))
                                var2 = int.Parse(i.Func.Arguments.Arguments[1].Name.ToLower().TrimStart('x'));
                            else
                                var2 = Vars[func.Name + i.Func.Arguments.Arguments[1].Name];

                            // Use the add function.
                            Mul(var1, var2);
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
                            int var1, var2;
                            // Check for the arguments var1
                            if (i.Func.Arguments.Arguments[0].Name.ToLower().StartsWith("x"))
                                var1 = int.Parse(i.Func.Arguments.Arguments[0].Name.ToLower().TrimStart('x'));
                            else
                                var1 = Vars[func.Name + i.Func.Arguments.Arguments[0].Name];

                            // Check for the arguments var2
                            if (i.Func.Arguments.Arguments[1].Name.ToLower().StartsWith("x"))
                                var2 = int.Parse(i.Func.Arguments.Arguments[1].Name.ToLower().TrimStart('x'));
                            else
                                var2 = Vars[func.Name + i.Func.Arguments.Arguments[1].Name];

                            // Use the add function.
                            Div(var1, var2);
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
                            int var1, var2;
                            // Check for the arguments var1
                            if (i.Func.Arguments.Arguments[0].Name.ToLower().StartsWith("x"))
                                var1 = int.Parse(i.Func.Arguments.Arguments[0].Name.ToLower().TrimStart('x'));
                            else
                                var1 = Vars[func.Name + i.Func.Arguments.Arguments[0].Name];

                            // Check for the arguments var2
                            if (i.Func.Arguments.Arguments[1].Name.ToLower().StartsWith("x"))
                                var2 = int.Parse(i.Func.Arguments.Arguments[1].Name.ToLower().TrimStart('x'));
                            else
                                var2 = Vars[func.Name + i.Func.Arguments.Arguments[1].Name];

                            // Use the add function.
                            Sub(var1, var2);
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
                            int var1, var2;
                            // Check for the arguments var1
                            if (i.Func.Arguments.Arguments[0].Name.ToLower().StartsWith("x"))
                                var1 = int.Parse(i.Func.Arguments.Arguments[0].Name.ToLower().TrimStart('x'));
                            else
                                var1 = Vars[func.Name + i.Func.Arguments.Arguments[0].Name];

                            // Check for the arguments var2
                            if (i.Func.Arguments.Arguments[1].Name.ToLower().StartsWith("x"))
                                var2 = int.Parse(i.Func.Arguments.Arguments[1].Name.ToLower().TrimStart('x'));
                            else
                                var2 = Vars[func.Name + i.Func.Arguments.Arguments[1].Name];

                            // Use the add function.
                            Add(var1,var2);
                        } else
                            // Thow an exception
                            throw new Exception("Invalid arguments in `proc_Add`.");
                        break;
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
                            int var0;
                            if (i.Func.Arguments.Arguments[0].Name.ToLower().StartsWith("x"))
                                //Set the var to a parsed integer of the var name without "x"
                                var0 = int.Parse(i.Func.Arguments.Arguments[0].Name.ToLower().TrimStart('x'));
                            else
                                // Set the var to the pre-defined value of the var name.
                                var0 = Vars[func.Name + i.Func.Arguments.Arguments[0].Name];

                            int var1;
                            if (i.Func.Arguments.Arguments[1].Name.ToLower().StartsWith("x"))
                                //Set the var to a parsed integer of the var name without "x"
                                var1 = int.Parse(i.Func.Arguments.Arguments[1].Name.ToLower().TrimStart('x'));
                            else
                                // Set the var to the pre-defined value of the var name.
                                var1 = Vars[func.Name + i.Func.Arguments.Arguments[1].Name];


                            Goto(var0,var1);
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
                            int var;
                            // If it starts with "x", directly load the value
                            if (i.Func.Arguments.Arguments[0].Name.ToLower().StartsWith("x"))
                                //Set the var to a parsed integer of the var name without "x"
                                var = int.Parse(i.Func.Arguments.Arguments[0].Name.ToLower().TrimStart('x'));
                            else
                                // Set the var to the pre-defined value of the var name.
                                var = Vars[func.Name + i.Func.Arguments.Arguments[0].Name];

                            // Clear the variable.
                            ClearVar(var);
                        }
                        else throw new Exception("Invalid arguments in `clear`.");
                        break;
                        /*
                         * Get value
                         */ 
				case "get":
						// Check for the arguments count
						if (i.Func.Arguments.Arguments.Count == 1) {
							int var;

							// If it starts with "x", directly load the value
							if (i.Func.Arguments.Arguments [0].Name.ToLower ().StartsWith ("x"))

								//Set the var to a parsed integer of the var name without "x"
								var = int.Parse (i.Func.Arguments.Arguments[0].Name.ToLower().TrimStart ('x')); 
							else
								// Set the var to the pre-defined value of the var name.
								var = Vars[func.Name + i.Func.Arguments.Arguments [0].Name];

							// Check if sould assign
							if (i.Op?.Type == "=") {
								// Check if the variable is valid
								if (i.Var?.isConst == false) {

									// If it starts with "x", directly put into the variable
									if (i.Var.Name.ToLower ().StartsWith ("x"))
										// Move.
										Move (int.Parse (i.Var.Name.ToLower ().TrimStart ('x')), var);
									else {
										// If the variable isn't defined, define it.
										if (!Vars.ContainsKey (func.Name + i.Var.Name)) {
											Vars.Add (func.Name + i.Var.Name, var);
										} else {
											// Change the variable
											Vars [func.Name + i.Var.Name] = var;
										}
									}
								}
							}
						} else
							// Throw the exception
							throw new Exception ("Incorrect amount of arguments in `get`.");
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
						} else
							// Thow the exception
							throw new Exception("Interrupt can only accept bytes as arguments.");
					break;
					/*
					 * Execute Function
					 */
					default:
						// Find the function
						Function f = find (i.Func.Namespace, i.Func.Function);
						// Set the function name to something unique to the namespace
						f.Name = i.Func.Namespace + " " + i.Func.Function + " ";
						// Generate the function, and return.
						Generate (f,i.Func.Arguments,func);
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
									else {
										// Define the variable if it's not already defined
										if (!Vars.ContainsKey(func.Name + i.Var.Name))
										{
                                            // Define Variable
                                            Vars.Add(func.Name + i.Var.Name, var);
                                        }
                                        else {
                                            // Change Variable
                                            Vars[func.Name + i.Var.Name] = var;
                                        }
                                    }
								}
							}
						}
						else {
							// Make sure that it's an invalid return and not a void
							if (f.Ret?.Type == TokenType.Number
								|| f.Ret?.Type == TokenType.Register
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

		/// <summary>
		/// Clears the variable.
		/// </summary>
		/// <param name="Var">Variable.</param>
		void ClearVar(int Var)
		{
			// Start Clear
			Output.Add(0x0);
			Output.Add(0xc);

			// Variable (as Bytes)
			foreach (byte add in BitConverter.GetBytes(Var))
				Output.Add(add);

			// End Clear
			Output.Add(0xff);
		}

		/// <summary>
		/// Frees the variable.
		/// </summary>
		/// <param name="Var">Variable.</param>
		void FreeVar(int Var)
        {
            ClearVar(Var);
            // When you make it available to the format, add it here too.
        }

        /// <summary>
        /// Collects the garbadge.
        /// </summary>
        /// <param name="FuncName">Function name.</param>
        void CollectGarbadge(string FuncName)
		{
			// To avoid a bug.
			var Varc = new Dictionary<string, int>(Vars);
			foreach (KeyValuePair<string,int> v in Varc)
			{
				if (v.Key.StartsWith(FuncName))
				{
					FreeVar(v.Value);
					Vars.Remove(v.Key);
				}
			}
		}

		/// <summary>
		/// Find the specified function.
		/// </summary>
		/// <param name="namesp">Namespace.</param>
		/// <param name="name">Function.</param>
		Function find(string namesp, string name)
		{
			// Check if the function is loaded
			if (Functions.ContainsKey (namesp + name))
				return Functions [namesp + name];

			// Nope, throw an exception
			throw new Exception ("Unable to find function `"+namesp+"."+name+"`");
		}

		/// <summary>
		/// Finds the main function.
		/// </summary>
		/// <returns>The main.</returns>
		Function findMain()
		{
			// Search for main
			foreach (Namespace name in Input.Namespaces)
				foreach (Function func in name.Functions)
					if (func.Name.ToLower () == "main")
						return func;

			// No main, throw an exception
			throw new Exception("No `Main` function was found!");
		}

		/// <summary>
		/// Get the result.
		/// </summary>
		public CodeGeneratorResult get(bool debug)
		{
			return new CodeGeneratorResult
			{
				Errors = Res.Errors,
				Warnings = Res.Warnings,
				Code = Output.ToArray(),
			};
		}
	}
}
