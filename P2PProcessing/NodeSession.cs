using P2PProcessing.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace P2PProcessing
{
    class NodeSession
    {
        Connection connection;

        public NodeSession(Connection connection)
        {
            this.connection = connection;

            new Thread(start).Start(); // TODO: store thread handle implement close(),  and join() on close()
        }

        private void start()
        {
            Console.WriteLine($"{this} started");
        }

        public override string ToString()
        {
            return $"Node session to {connection}";
        }
    }
}
