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

        // TODO: StartProblem

        // TODO: GetProgressInfo
    }
}
