using P2PProcessing.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace P2PProcessing.Protocol
{
    class Connection
    {
        public const int HEADER_SIZE = 6;

        public string Host;
        public int Port;
        public Guid id;

        private Socket socket;

        public static Connection To(string host, int port, Guid id)
        {
            Connection connection = new Connection();
            connection.Host = host;
            connection.Port = port;
            connection.id = id;

            connection.initialize();
            connection.Send(new HelloMsg());

            return connection;
        }

        public static Connection From(Socket socket, Guid id)
        {
            Connection connection = new Connection();
            var endpoint = (IPEndPoint)(socket.RemoteEndPoint);

            connection.Host = endpoint.Address.ToString();
            connection.Port = endpoint.Port;
            connection.id = id;

            connection.socket = socket;

            return connection;
        }

        public Msg Receive()
        {
            try
            {
                byte[] header = new byte[HEADER_SIZE];
                socket.Receive(header, HEADER_SIZE, SocketFlags.None);

                MsgBuffer msgBuffer = new MsgBuffer(header);
                P2P.logger.Debug($"{this}: Received {msgBuffer.kind} header");

                byte[] body = new byte[msgBuffer.bodyLength];

                socket.Receive(body, (int)msgBuffer.bodyLength, SocketFlags.None);
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
                socket.Send(buffer);
            } catch (Exception e)
            {
                P2P.logger.Error($"Error sending message: {e}");
                throw new ConnectionException($"Error while sending message {msg.GetMsgKind()}");
            }

        }

        public void Close()
        {
            // TODO: create new Msg Disconnect, to inform other nodes that i'm disconecting

            P2P.logger.Debug($"{this} ending..");
            this.socket.Close();
        }

        public HelloResponseMsg ListenForHelloResponse()
        {
            // TODO: with timeout

            var message = this.Receive() as HelloResponseMsg;

            if (message == null)
            {
                throw new ProtocolException("Received message, but kind wasn't equal to HelloResponse");
            }

            return message;
        }  
        
        public HelloMsg ListenForHello()
        {
            // TODO: with timeout

            var message = this.Receive() as HelloMsg;

            if (message == null)
            {
                throw new ProtocolException("Received message, but kind wasn't equal to Hello");
            }

            return message;
        }

        private void initialize()
        {
            try
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(this.Host, this.Port);
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
