using P2PProcessing.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using P2PProcessing.States;
using P2PProcessing.Problems;
using P2PProcessing.Utils;

namespace P2PProcessing
{
    public class Session
    {
        Dictionary<Guid, NodeSession> connectedSessions = new Dictionary<Guid, NodeSession>();
        Guid id = Guid.NewGuid();
        TcpListener listener;
        Thread listenerThread;
        State state;
        SocketConnectionFactory connectionFactory = new SocketConnectionFactory();

        public Problem currentProblem;

        public Session(int port)
        {
            this.ChangeState(new NotWorkingState(this));
            this.discoverNodes(port);
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

        public void RemoveNode(Guid id)
        {
            connectedSessions.Remove(id);
        }

        private void discoverNodes(int ownPort)
        {
            P2P.logger.Info($"{this}: Starting discovery process");
            try
            {
                for (int i = 5100; i < 5200; i += 1)
                {
                    if (i == ownPort) continue;

                    unsafeConnectToNode("localhost", i);
                }
            }
            catch
            {
                P2P.logger.Info($"{this}: Ended discover process");
            }
        }


        private void unsafeConnectToNode(string host, int port)
        {
            Connection connection = connectionFactory.createOutgoingConnection(host, port, id);
            connection.Send(new HelloMsg());

            var helloResponse = connection.ListenForHelloResponse();

            connectedSessions.Add(helloResponse.GetNodeId(), new NodeSession(this, connection, helloResponse.GetNodeId()));
            P2P.logger.Info($"{this}: Connected to: {helloResponse.GetNodeId()}");
        }

        public void ConnectToNode(string host, int port)
        {
            try
            {
                unsafeConnectToNode(host, port);
            }
            catch (Exception e)
            {
                P2P.logger.Error($"{this}: Couldn't connect: e.Message");
            }
        }

        public void BroadcastToConnectedNodes(Msg msg)
        {
            P2P.logger.Debug($"{this}: Broadcasting message {msg.GetType()}");
            foreach (var nodeSession in this.connectedSessions.Values)
            {
                nodeSession.Send(msg);
            }
        }

        public void OnMessage(Msg msg)
        {
            P2P.logger.Debug($"{this}: Message {msg.GetMsgKind()} from {msg.GetNodeId()} received");

            state.OnMessage(msg);
        }

        public void ChangeState(State state)
        {
            if (this.state != null)
            {
                this.state.EndCalculating();
            }
            this.state = state;
        }

        private void listenForConnections()
        {
            P2P.logger.Info($"{this} listening for connections {listener.LocalEndpoint}..");

            while (true)
            {
                Socket socket = listener.AcceptSocket();

                P2P.logger.Debug($"{this}: Received connection");

                var endpoint = (IPEndPoint)(socket.RemoteEndPoint);

                Connection connection = connectionFactory.createConnection(endpoint.Address.ToString(), endpoint.Port, id);

                if (connection is SocketConnection)
                {
                    (connection as SocketConnection).Socket = socket;
                }

                var hello = connection.ListenForHello();
                connection.Send(new HelloResponseMsg());

                connectedSessions.Add(hello.GetNodeId(), new NodeSession(this, connection, hello.GetNodeId()));
                P2P.logger.Info($"{this}: Received connection from node: {hello.GetNodeId()}");
            }
        }
        
        public void HandlePayloadCalculated(int payloadIndex, string result)
        {
            this.currentProblem.SetPayloadState(payloadIndex, new Calculated());
            if (string.IsNullOrEmpty(result) && this.currentProblem.Solution == null)
            {
                BroadcastToConnectedNodes(ProblemUpdatedMsg.FromProblem(this.currentProblem));
                P2P.logger.Info($"Calculated another payload, no success, checked {currentProblem.GetProgress()}% payloads");

                state.CalculateNext();
            }
            else if (!string.IsNullOrEmpty(result))
            {
                this.ChangeState(new NotWorkingState(this));
                P2P.logger.Info($"Found correct combination: {result} for hash: {currentProblem.Hash}\nChecked {currentProblem.GetProgress()}% payloads");

                lock (this.currentProblem)
                {
                    this.currentProblem.Solution = result;

                    BroadcastToConnectedNodes(ProblemSolvedMsg.FromResult(result, this.currentProblem));
                }
            }

        }

        public void DetermineAction()
        {
            if (this.currentProblem != null)
            {
                lock (this.currentProblem)
                {
                    if (this.currentProblem.Solution == null && this.state is NotWorkingState)
                    {
                        this.ChangeState(new WorkingState(this));
                        this.state.CalculateNext();
                    }
                    else if (this.currentProblem.Solution == null)
                    {
                        this.state.CalculateNext();
                    }
                }
            }
        }

        public void SetProblem(string hash)
        {
            P2P.logger.Info($"Setting problem {hash}...");
            var problem = ProblemCalculation.CreateProblemFromHash(hash);

            this.state.OnMessage(ProblemUpdatedMsg.FromProblem(problem));
        }

        public int GetProgress()
        {
            return currentProblem.GetProgress();
        }

        public override string ToString()
        {
            return $"Session {id}";
        }
    }

}
