using System;
using P2PProcessing;
using P2PProcessingConsole;

namespace P2PPlaceholderNode
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var p = new P2P(1234, new Log(Level.Debug));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
