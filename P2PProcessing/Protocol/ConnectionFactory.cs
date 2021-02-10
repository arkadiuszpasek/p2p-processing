using System;
using System.Collections.Generic;
using System.Text;

namespace P2PProcessing.Protocol
{
    public abstract class ConnectionFactory
    {
        public abstract Connection createConnection(string host, int port, Guid ownId);
    }
}
