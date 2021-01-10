using P2PProcessing.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace P2PProcessing
{
    class Session
    {
        Dictionary<Guid, NodeSession> sessions = new Dictionary<Guid, NodeSession>();
        Guid id = Guid.NewGuid();
        TcpListener listener;

        public Session(int port)
        {
            listener = new TcpListener(IPAddress.Any, port); // TODO: make config and move port there
            listener.Start();

            new Thread(listenForConnections).Start(); // TODO: store in class and add Close & finish
        }

        public void ConnectToNode(string host, int port)
        {
            Connection connection = Connection.To(host, port, id);

            var helloResponse = connection.ListenForHelloResponse();

            sessions.Add(helloResponse.GetNodeId(), new NodeSession(connection));
        }


        private void listenForConnections()
        {
            Console.WriteLine($"{this} listening for connections..");

            while (true)
            {
                Socket socket = listener.AcceptSocket();

                Connection connection = Connection.From(socket, id);
                var helloMsg = connection.ListenForHello();

                sessions.Add(helloMsg.GetNodeId(), new NodeSession(connection));
            }
        }

        public override string ToString()
        {
            return $"Session {id}";
        }
    }

}
