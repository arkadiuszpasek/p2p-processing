﻿using System;
using P2PProcessing;

namespace P2PProcessingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            const string input = "Hello";
            try
            {
                var p = new P2P(8889, new Log(Level.Info));
                p.ConnectToNode("localhost", 1234);

                Console.WriteLine($"Running test input: {input}");
                p.SetProblemRaw(input);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
