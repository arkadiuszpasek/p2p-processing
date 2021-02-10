using P2PProcessing.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

                Socket.Receive(body, (int)msgBuffer.bodyLength, SocketFlags.None);
                P2P.logger.Debug($"{this}: Body of {msgBuffer.kind} received - {msgBuffer.bodyLength}");
                return msgBuffer.BodyToMsg(body);
            } catch (Exception e)
            {
                P2P.logger.Error($"Error listening for message: {e}");
                throw new ConnectionException("Receiving error");
            }
        }

        public void Send(Msg msg)
        {
            msg.SetNodeId(this.id);

            try
            {
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
                this.Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                Socket.Connect(this.Host, this.Port);
                P2P.logger.Debug($"Connection connected to {Host}:{Port}");
            }
            catch (Exception e)
            {
                P2P.logger.Error($"Error intializing socket to: {Host}: {e.Message}");
                throw new ConnectionException("Error initializing");
            }
        }

        public override string ToString()
        {
            return $"Connection: [{Host}:{Port}]";
        }

    }
}
