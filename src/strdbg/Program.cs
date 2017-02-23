using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace strdbg
{
    class Program
    {
        static void Main()
        {
            while(true)
            {
                if (!Console.IsOutputRedirected)
                {
                    Console.WriteLine("Output is not redirected!");
                    return;
                }
                if (!Console.IsInputRedirected)
                {
                    Console.WriteLine("Input is not redirected!");
                    return;
                }
                if (!Console.IsErrorRedirected)
                {
                    Console.WriteLine("Error is not redirected!");
                    return;
                }
                Thread.Sleep(10);
            }
        }
    }
}
