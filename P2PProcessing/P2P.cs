using System;
using System.Collections.Generic;
using P2PProcessing.Protocol;

namespace P2PProcessing
{
    public class P2P
    {
        Session session;

        public P2P(int port)
        {
            this.session = new Session(port);
        }


        public void ConnectToNode(string host, int port)
        {
            session.ConnectToNode(host, port);
        }
    }
}
