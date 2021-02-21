﻿using System;
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
                if (input.Length > 10 || input.Length < 2)
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
                int port = 5100;
                if (args.Length > 0)
                {
                    port = int.Parse(args[0]);
                }

                var p = new P2P(port, new Log(Level.Info));
                
                while (true)
                {
                    p.SetProblemRaw(Program.getProblemString(), 2, 10);

                    while (string.IsNullOrEmpty(p.GetSession().currentProblem.Solution))
                    {
                        Console.ReadLine();
                        Console.WriteLine($"Calculated payloads: {p.GetProgress()}%");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            
        }
    }
}
