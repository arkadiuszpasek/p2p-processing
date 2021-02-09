using System;
using System.Collections.Generic;
using P2PProcessing;
using P2PProcessingConsole;
using P2PProcessing.Utils;

namespace P2PPlaceholderNode
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var p = new P2P(1234, new Log(Level.Info));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
