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

        public void SetProblemRaw(string inputToHash, int minLength, int maxLength)
        {
            this.SetProblemHash(Hasher.getHashHexRepresentation(inputToHash), minLength, maxLength);
        }

        public void SetProblemHash(string hash, int minLength, int maxLength)
        {
            session.SetProblem(hash, minLength, maxLength);
        }

        public int GetProgress()
        {
            return session.GetProgress();
        }

        public Session GetSession()
        {
            return this.session;
        }
    }
}
