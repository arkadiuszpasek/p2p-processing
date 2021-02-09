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

        Problem[] history; // może timestampy?
        public Problem currentProblem;

        public Session(int port)
        {
            this.ChangeState(new NotWorkingState(this));
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
            P2P.logger.Info($"Broadcasting message {msg.GetType()}");
            foreach (var nodeSession in this.connectedSessions.Values)
            {
                nodeSession.Send(msg);
            }
        }

        public void OnMessage(Msg msg)
        {
            P2P.logger.Info($"{this}: Message {msg.GetMsgKind()} from {msg.GetNodeId()} received");

            state.OnMessage(msg);
        }

        public void ChangeState(State state)
        {
            this.state = state;
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
        
        public void HandlePayloadCalculated(int payloadIndex, string result)
        {
            this.currentProblem.SetPayloadState(payloadIndex, new Calculated());
            if (result == null)
            {
                BroadcastToConnectedNodes(ProblemUpdatedMsg.FromProblem(this.currentProblem));
                P2P.logger.Debug($"Calculated another payload, still no success..{state}");
                state.CalculateNext();
            }
            else
            {
                P2P.logger.Info($"Found correct combination: {result} for hash: {currentProblem.Hash}. {currentProblem}");

                BroadcastToConnectedNodes(ProblemSolvedMsg.FromResult(result, this.currentProblem));
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
