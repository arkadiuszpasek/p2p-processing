﻿using P2PProcessing.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using P2PProcessing.States;
using P2PProcessing.Problems;
using P2PProcessing.Utils;
using System.Net.NetworkInformation;
using System.Text;

namespace P2PProcessing
{
    public class Session
    {
        Dictionary<Guid, NodeSession> connectedSessions = new Dictionary<Guid, NodeSession>();
        Guid id = Guid.NewGuid();
        TcpListener listener;
        UdpClient udpClient;
        Thread listenerThread;
        Thread udpThread;
        State state;
        SocketConnectionFactory connectionFactory = new SocketConnectionFactory();

        public Problem currentProblem;

        public Session(int port)
        {
            this.ChangeState(new NotWorkingState(this));
            this.listener = new TcpListener(IPAddress.Any, port);
            this.listener.Start();

            this.udpClient = new UdpClient(port);
            this.udpClient.EnableBroadcast = true;

            this.listenerThread = new Thread(listenForConnections);
            this.listenerThread.Start();

            this.udpThread = new Thread(() => listenerForBroadcasts(port));
            this.udpThread.Start();

            this.discoverNodes(port);
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

            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, ownPort);
            byte[] bytes = Broadcast.WhoIsPresentMsg;
            this.udpClient.Send(bytes, bytes.Length, ip);

            this.discoverLocalNodes(ownPort);
        }

        private void discoverLocalNodes(int ownPort)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var connections = ipGlobalProperties.GetActiveUdpListeners();

            foreach (var connection in connections)
            {
                if (connection.Port >= 5100 && connection.Port <= 5200 && ownPort != connection.Port)
                {
                    try
                    {
                        IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, ownPort);
                        byte[] bytes = Broadcast.WhoIsPresentMsg;
                        this.udpClient.Send(bytes, bytes.Length, ip);
                        connectToNodeAt(connection.Address.MapToIPv4().ToString(), connection.Port);
                    }
                    catch (Exception e)
                    {
                        P2P.logger.Debug($"Connecting status {e.Message}");
                    }
                }
            }
        }

        private void connectToNodeAt(string host, int port)
        {
            if (host == "0.0.0.0")
            {
                host = "127.0.0.1";
            }
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
                connectToNodeAt(host, port);
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

        private void listenerForBroadcasts(int port)
        {
            while (true)
            {
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);
                var msg = this.udpClient.Receive(ref groupEP);

                if (msg == Broadcast.WhoIsPresentMsg)
                {
                    P2P.logger.Debug("A node is asking for present nodes");
                    byte[] response = Broadcast.IAmPresentMsg(port);
                    this.udpClient.Send(response, response.Length, groupEP);
                }
                else if (Broadcast.isIAmPresentMsg(msg))
                {
                    P2P.logger.Debug("A node is telling its present");
                    var p = Broadcast.parsePresentMsg(msg);
                    this.ConnectToNode(groupEP.Address.ToString(), p);
                }
            }
        }

        private void listenForConnections()
        {
            P2P.logger.Debug($"{this} listening for connections {listener.LocalEndpoint}..");

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
