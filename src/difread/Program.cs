using System;
namespace difread
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string s in args)
                try
                {
                    new Read(s);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed reading file {0}: {1}", s, e.Message);
                }
        }
    }
}
