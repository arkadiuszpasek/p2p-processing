using System;
using System.Diagnostics;
using System.Threading;

namespace NodeStarter
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                const string nodeProgramPath = @"D:\Dev\dp\P2PProcessing\P2PProcessingConsole\bin\Debug\netcoreapp3.1\P2PProcessingConsole.exe";
                Console.Write("Number of nodes to start: ");
                string n = Console.ReadLine();
                int number = int.Parse(n);
                for (int i = 0; i < number; i += 1)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = nodeProgramPath;
                    p.StartInfo.Arguments = (5100 + i).ToString();
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    p.StartInfo.CreateNoWindow = false;
                    p.Start();
                    Thread.Sleep(2500);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
