using P2PProcessing.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace P2PProcessing
{

    abstract class State
    {
        Session session;
        abstract public void onMessage(Msg msg);
    }

    // class WorkingState : State {
    // onMessage(Msg) -> biorę wolny -> wysyłam Updated, this.session.broadcast(Msg)
    // }


    class Session
    {
        Dictionary<Guid, NodeSession> connectedSessions = new Dictionary<Guid, NodeSession>();
        Guid id = Guid.NewGuid();
        TcpListener listener;
        Thread listenerThread;
        State state;

        // Problem[] history; // Potencjalnie snapshot, może timestampy?
        // Problem currentProblem; Aktualny? 

        abstract class PayloadState { }

        class Free : PayloadState { }
        class Taken : PayloadState
        {
            long timestamp;
            public Taken()
            {
                // TODO: 
                // this.timestamp = Clock.now()?
            }
        }
        class Calculated : PayloadState { }

        class Problem 
        {
            string hash;
            PayloadState[] assignement;

            public int GetProgress()
            {
                // TODO: przy eksportowaniu do osobnego pliku dodać using System.Linq;
                return assignement.Aggregate(0, (acc, payload) => payload is Calculated ? acc + 1 : acc);
            }

            // ProblemUpdated[ 1 -> Taken(timestamp), 2 -> free, 3 -> free,  4 -> free  ]
            // ProblemUpdated[ 1 -> Taken(timestamp), 2 -> free, 3 -> free,  4 -> free  ]
            // ./filter(_.free) <- ProblemUpdated[ 1 -> Taken(timestamp), 2 -> Taken(timestamp), 3 -> free,  4 -> free  ]

            // ProblemUpdated[ 1 -> Taken(timestamp), 2 -> Taken(timestamp), 3 -> free,  4 -> free  ]
            // ProblemUpdated[1-> Calculated, 2->Taken(timestamp), 3->Calculated, 4->Calculated] <- bierzemy pierwszy Taken, jeśli nie ma Free
        }

        public Session(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            this.listenerThread = new Thread(listenForConnections);
            this.listenerThread.Start();
        }

        public void Close()
        {
            P2P.logger.Debug($"{this} ending..");

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
            P2P.logger.Debug($"{this}: Connecting to: {host}:{port}");
            Connection connection = Connection.To(host, port, id);

            var helloResponse = connection.ListenForHelloResponse();

            P2P.logger.Info($"{this}: Connected to: {helloResponse.GetNodeId()}");

            connectedSessions.Add(helloResponse.GetNodeId(), new NodeSession(this, connection));
        }

        public void BroadcastToConnectedNodes(Msg msg)
        {
            foreach (var nodeSession in this.connectedSessions.Values)
            {
                nodeSession.Send(msg);
            }
        }

        public void OnMessage(Msg msg)
        {
            P2P.logger.Debug($"{this}: Message {msg.GetMsgKind()} from {msg.GetNodeId()} received");

            //state.onMessage(msg)
        }

        private void listenForConnections()
        {
            P2P.logger.Info($"{this} listening for connections..");

            while (true)
            {
                Socket socket = listener.AcceptSocket();

                P2P.logger.Debug($"{this}: Received connection");

                Connection connection = Connection.From(socket, id);
                var hello = connection.ListenForHello();
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
