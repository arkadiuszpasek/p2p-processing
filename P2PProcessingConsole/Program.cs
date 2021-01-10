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
                var p = new P2P();
                //p.ConnectToNode("localhost", 8888);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
