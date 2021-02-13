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
                var p = new P2P(1235, new Log(Level.Info));
                
                // Connect to a node that's already running on this given port:
                p.ConnectToNode("localhost", 1234);

                p.SetProblemRaw(p.inputProblem()); //hash
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }
    }
}
