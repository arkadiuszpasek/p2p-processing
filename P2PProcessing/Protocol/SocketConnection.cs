using P2PProcessing.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace P2PProcessing.Protocol
{
    class SocketConnection : Connection
    {
        public const int HEADER_SIZE = 6;

        public string Host;
        public int Port;
        public Socket Socket;

        private Guid id;

        public SocketConnection(string host, int port, Guid ownId)
        {
            this.Host = host;
            this.Port = port;
            this.id = ownId;
        }

        public Msg Receive()
        {
            try
            {
                byte[] header = new byte[HEADER_SIZE];
                Socket.Receive(header, HEADER_SIZE, SocketFlags.None);

                MsgBuffer msgBuffer = new MsgBuffer(header);
                P2P.logger.Debug($"{this}: Received {msgBuffer.kind} header");

                byte[] body = new byte[msgBuffer.bodyLength];

                int n = Socket.Receive(body, (int)msgBuffer.bodyLength, SocketFlags.None);
                if (n != msgBuffer.bodyLength)
                {
                    throw new ConnectionException($"Didn't receive full body length {n}, expected {msgBuffer.bodyLength}");
                }
                P2P.logger.Debug($"{this}: Body of {msgBuffer.kind} received - {msgBuffer.bodyLength}");
                return msgBuffer.BodyToMsg(body);
            } catch (Exception e)
            {
                P2P.logger.Warn($"Connection has disconnected {e.Message}");
                throw new ConnectionException("Receiving error");
            }
        }

        public void Send(Msg msg)
        {
            msg.SetNodeId(this.id);

            try
            {
                Thread.Sleep(100);
                byte[] buffer = MsgBuffer.MsgToBuffer(msg);
                P2P.logger.Debug($"{this}: Sending {msg.GetMsgKind()} message - {buffer.Length}");
                Socket.Send(buffer);
                
            } catch (Exception e)
            {
                P2P.logger.Error($"Error sending message: {e}");
                throw new ConnectionException($"Error while sending message {msg.GetMsgKind()}");
            }

        }

        public void Close()
        {
            P2P.logger.Debug($"{this} ending..");
            this.Socket.Close();
        }

        public HelloResponseMsg ListenForHelloResponse()
        {
            var message = this.Receive() as HelloResponseMsg;

            if (message == null)
            {
                throw new ProtocolException("Received message, but kind wasn't equal to HelloResponse");
            }

            return message;
        }  
        
        public HelloMsg ListenForHello()
        {
            var message = this.Receive() as HelloMsg;

            if (message == null)
            {
                throw new ProtocolException("Received message, but kind wasn't equal to Hello");
            }

            return message;
        }

        public void Initialize()
        {
            try
            {
                this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IAsyncResult result = this.Socket.BeginConnect(this.Host, this.Port, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(1500, true);

                if (this.Socket.Connected)
                {
                    this.Socket.EndConnect(result);
                    P2P.logger.Debug($"Connection connected to {Host}:{Port}");
                }
                else
                {
                    this.Socket.Close();
                    throw new ConnectionException($"Failed to connect to {this.Host}:{this.Port}");
                }
            }
            catch
            {
                throw new ConnectionException("Error initializing");
            }
        }

        public override string ToString()
        {
            return $"Connection: [{Host}:{Port}]";
        }

    }
}
