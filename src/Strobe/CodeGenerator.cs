using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace Strobe
{
	/// <summary>
	/// Code generator.
	/// </summary>
	public partial class CodeGenerator
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
        /// Initializes a new instance of the <see cref="CodeGenerator"/> class.
        /// </summary>
        /// <param name="Input">Input.</param>
        public CodeGenerator (ParseTree Input)
		{
			// Changed to 16 because we seperated the memory of the processes
			CurrentVar = 16;
			this.Input = Input;
			// Generate the code.
			Generate();
		}

        /// <summary>
        /// Add the DIF Header to the file
        /// </summary>
        void Header()
        {
            // Not actually needed, but recommended
            foreach (byte add in "DIF" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion)
                Output.Add(add);
        }

        /// <summary>
        /// Generate the code.
        /// </summary>
        void Generate()
		{
            // Header
            Header();

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
        /// Get a passed argument from 
        /// </summary>
        /// <param name="i">Current Instruction</param>
        /// <param name="func">Current Function</param>
        /// <param name="num"></param>
        /// <returns>The </returns>
        int GetArgument(Instruction i,Function func, int num)
        {
            int ret;
            if (i.Func.Arguments.Arguments[num].Name.ToLower().StartsWith("x"))
                ret = int.Parse(i.Func.Arguments.Arguments[num].Name.ToLower().TrimStart('x'));
            else
                ret = Vars[func.Name + i.Func.Arguments.Arguments[num].Name];

            return ret;
        }

        /// <summary>
        /// Preprocess the specified string.
        /// </summary>
        /// <param name="s">String.</param>
        void Preprocess(string s)
		{
			// Hell.. (NO LONGER HELL)

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
            foreach (Instruction i in func.Instructions)
            {
                GenerateInstruction(i,func,args,old);
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
