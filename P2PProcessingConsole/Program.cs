using System;
using P2PProcessing;

namespace P2PProcessingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputProblem()
            {
                string input = "";
                bool incorrect = true;
                Console.WriteLine("Enter our password to hash it (only english letters and length has to be between 2 and 5)");
                while (incorrect)
                {
                    input = Console.ReadLine();
                    if (input.Length > 5 || input.Length < 2)
                    {
                        Console.WriteLine("Your input is invalid, try again, remember about the rules!");
                        Console.WriteLine("Enter our password to hash it (only english letters and length has to be between 2 and 5)");
                    }
                    else
                    {
                        Console.WriteLine("Your input: {0}", input);
                        incorrect = false;
                    }
                }
                return input;
            }

            try
            {
                var p = new P2P(1235, new Log(Level.Info));
                
                // Connect to a node that's already running on this given port:
                p.ConnectToNode("localhost", 1234);

                p.SetProblemRaw(inputProblem()); //hash
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
