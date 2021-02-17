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
    }
}
