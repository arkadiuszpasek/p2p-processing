using System;
using P2PProcessing;

namespace P2PProcessingConsole
{
    class Program
    {
        public static string getProblemString()
        {
            while (true)
            {
                Console.WriteLine("Enter our password to hash it (only english letters and length has to be between 2 and 5)");
                string input = Console.ReadLine();
                if (input.Length > 5 || input.Length < 2)
                {
                    Console.WriteLine("Your input is invalid, try again, remember about the rules!");
                }
                else
                {
                    Console.WriteLine("Your input: {0}", input);
                    return input;
                }
            }
        }
        static void Main(string[] args)
        {
            try
            {
                if (!(args.Length > 0))
                {
                    Console.WriteLine("Starting arguments not provided");
                    return;
                }

                int port = int.Parse(args[0]);
                var p = new P2P(port, new Log(Level.Info));
                
                p.SetProblemRaw(Program.getProblemString());

                while (true)
                {
                    Console.ReadLine();
                    Console.WriteLine($"Calculated payloads: {p.GetProgress()}%");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
