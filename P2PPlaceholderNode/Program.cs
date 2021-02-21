using System;
using System.Threading;
using Newtonsoft.Json;
using P2PProcessing;
using P2PProcessing.Problems;
using P2PProcessing.Protocol;
using P2PProcessing.Utils;

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
                if (input.Length > 5 || input.Length < 2)
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
                var ha = Hasher.getHashHexRepresentation("hello");
                Console.WriteLine(ha);
                var probl = ProblemCalculation.CreateProblemFromHash(ha);
                var updated = ProblemUpdatedMsg.FromProblem(probl);

                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var json = JsonConvert.SerializeObject(updated, settings);
                Console.WriteLine(json);

                var msg = JsonConvert.DeserializeObject<Msg>(json, settings);
                var x = msg as ProblemUpdatedMsg;
                if (x != null)
                {
                    Console.WriteLine("Yeah");
                    Console.WriteLine(x.Problem.Hash);
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
