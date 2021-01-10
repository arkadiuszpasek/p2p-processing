using System;
using System.Collections.Generic;
using P2PProcessing.Protocol;

namespace P2PProcessing
{
    public class P2P
    {
        Session session = new Session(8888);

        public void ConnectToNode(string host, int port)
        {
            session.ConnectToNode(host, port);
        }
    }
}
