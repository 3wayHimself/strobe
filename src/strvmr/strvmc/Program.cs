using System.IO;
using System;
using strobe.runtime.DIF;
using System.Collections.Generic;
using strobe.runtime;
using strvmc.APIs;

namespace strvmc
{
	/// <summary>
	/// Strobe VMC.
	/// </summary>
	public class StrobeVMC
	{
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="param">The command-line arguments.</param>
		public static void Main(string[] param)
		{
            // Will find a solution for this (to add them automaticaly).
            API framework = new Framework(
                new API[] {
                    new StrobeAPI(),
                    // If you make an API add it here!
                }, "Internal APIs");

            bool debug = false;
            int memorysize = 2048;
            int lastCode = 0;

            // List of processes
            List<Process> proc = new List<Process>();

			// Check for the console input
			foreach (string s in param)
			{
				// Switch the string
				switch (s.ToLower())
				{
                  // Show out the parsed executable
                case "--debug":
                    debug = true;
                    break;
					// 512MB memory
				case "--512m":
                    memorysize = (1024 * 1024 * 512);
					break;
					// 32MB memory
				case "--32m":
                    memorysize = (1024 * 1024 * 32);
					break;
					// 1MB memory
				case "--1m":
                    memorysize = (1024 * 1024);
					break;
					// 1GB memory
				case "--1g":
                    memorysize = (1024 * 1024 * 1024);
					break;
					// Load File
					default:
						try
						{
                            var x = new DIFFormat().Load(File.ReadAllBytes(s));
                            if (debug)
                            {
                                    foreach (var y in x.CPU())
                                    {
                                        Console.WriteLine("{0}: {1}", y.Op, Convert.ToBase64String(y.Param));
                                    }
                            }
                            // Start the application
                            proc.Add(new Process(x,memorysize,framework));
						}
						catch(Exception e)
						{
                            if (s.Length > 2)
                            // Show output
                            Console.WriteLine("{1} @ {0}", s, e.Message);
							// Error while loading, increase the counter
						}
					break;
				}
			}

			// Step while it has running processes...
			while (proc.Count > 0)
			{
                for (int i = 0; i < proc.Count; i++)
                {
                    if (!proc[i].IsRunning())
                    {
                        proc.RemoveAt(i);
                        continue;
                    }
                    try
                    {
                        proc[i].Step();
                    }
                    catch (ApplicationNotRunning)
                    {
                        proc[i].Kill();
                    }
                    catch (ApplicationExit s)
                    {
                        lastCode = int.Parse(s.Message);
                        proc[i].Kill();
                    }
                    catch (ApplicationError e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Application Error: {0}",e.Message);
                        Console.ResetColor();
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unhandled Exception: {0}", e.Message);
                        Console.ResetColor();
                    }
                }
            }
		}
	}
}
