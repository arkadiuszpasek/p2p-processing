using System;
using P2PProcessing;

namespace P2PProcessingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var p = new P2P(8889, new Log(Level.Info));
                p.ConnectToNode("localhost", 1234);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
