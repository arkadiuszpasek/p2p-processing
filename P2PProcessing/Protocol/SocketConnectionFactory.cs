using System;
using System.Collections.Generic;
using System.Text;

namespace P2PProcessing.Protocol
{
    class SocketConnectionFactory : ConnectionFactory
    {
        public override Connection createConnection(string host, int port, Guid ownId)
        {
            return new SocketConnection(host, port, ownId);
        }

        public override Connection createOutgoingConnection(string host, int port, Guid ownId)
        {
            var connection = new SocketConnection(host, port, ownId);
            connection.Initialize();

            return connection;
        }
    }
}
