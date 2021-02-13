using System;
using System.Collections.Generic;
using P2PProcessing.Protocol;
using P2PProcessing.Utils;

namespace P2PProcessing
{
    public class P2P
    {
        public static Logger logger = new DefaultLogger();

        Session session;

        public P2P(int port, Logger logger)
        {
            this.session = new Session(port);
            P2P.logger = logger;
        }
        public P2P(int port)
        {
            this.session = new Session(port);
        }


        public void ConnectToNode(string host, int port)
        {
            session.ConnectToNode(host, port);
        }

        public void SetProblemRaw(string inputToHash)
        {
            this.SetProblemHash(Hasher.getHashHexRepresentation(inputToHash));
        }

        public void SetProblemHash(string hash)
        {
            session.SetProblem(hash);
        }

        public int GetProgress()
        {
            return session.GetProgress();
        }

        public string inputProblem()
        {
            string input = "";
            bool incorrect = true;
            Console.WriteLine("Enter our password to hash it (only english letters and length has to be between 2 and 5)");
            while (incorrect)
            {
                input = Console.ReadLine();
                if (input.Length > 5 || input.Length < 2)
                {
                    Console.WriteLine("Your input is invalid, try again, remember about the rules!");
                    Console.WriteLine("Enter our password to hash it (only english letters and length has to be between 2 and 5)");
                }
                else
                {
                    Console.WriteLine("Your input: {0}", input);
                    incorrect = false;
                }
            }
            return input;
        }
    }
}
