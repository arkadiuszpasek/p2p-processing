using System;
using System.Threading;
using Newtonsoft.Json;
using P2PProcessing;
using P2PProcessing.Problems;

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
                var free = Free.Of(5, "s");
                Console.WriteLine(JsonConvert.SerializeObject(free));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
