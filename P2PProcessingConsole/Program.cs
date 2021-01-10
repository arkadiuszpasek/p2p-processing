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
                var first = false;
                if (first)
                {
                    var p = new P2P(8889);
                    p.ConnectToNode("localhost", 8888);
                }
                else
                {
                    var p = new P2P(8888);
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
