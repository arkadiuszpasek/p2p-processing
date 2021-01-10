using System;
using P2PProcessing;

namespace P2PProcessingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new P2PSdk();
            string isItWorking = p.Start();
            Console.WriteLine(isItWorking);
        }
    }
}
