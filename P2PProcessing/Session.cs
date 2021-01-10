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
        Dictionary<Guid, NodeSession> connectedSessions = new Dictionary<Guid, NodeSession>();
        Guid id = Guid.NewGuid();
        TcpListener listener;
        Thread listenerThread;

        public Session(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            this.listenerThread = new Thread(listenForConnections);
            this.listenerThread.Start();
        }

        public void Close()
        {
            Console.WriteLine($"{this} ending..");

            if (listenerThread.IsAlive)
            {
                listenerThread.Join();
            }

            foreach (var connection in connectedSessions)
            {
                connection.Value.Close();
            }
        }

        public void ConnectToNode(string host, int port)
        {
            Console.WriteLine($"{this}: Connecting to: {host}:{port}");
            Connection connection = Connection.To(host, port, id);

            var helloResponse = connection.ListenForHelloResponse();

            Console.WriteLine($"{this}: Connected to: {helloResponse.GetNodeId()}");

            connectedSessions.Add(helloResponse.GetNodeId(), new NodeSession(this, connection));
        }

        public  void onMessage(Msg msg)
        {
            Console.WriteLine($"{this}: Message {msg.GetMsgKind()} from {msg.GetNodeId()} received");
        }

        private void listenForConnections()
        {
            Console.WriteLine($"{this} listening for connections..");

            while (true)
            {
                Socket socket = listener.AcceptSocket();

                Console.WriteLine($"{this}: Received connection");

                Connection connection = Connection.From(socket, id);
                var hello = connection.ListenForHello(); // TDOO: TIMEOUT
                connection.Send(new HelloResponseMsg());

                connectedSessions.Add(hello.GetNodeId(), new NodeSession(this, connection));
            }
        }


        public override string ToString()
        {
            return $"Session {id}";
        }
    }

}
