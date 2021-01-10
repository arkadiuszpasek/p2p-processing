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
            // TODO: wrap in try catch and loggin
            byte[] header = new byte[HEADER_SIZE];
            socket.Receive(header, HEADER_SIZE, SocketFlags.None);

            MsgBuffer msgBuffer = new MsgBuffer(header);
            Console.WriteLine($"{this}: Received {msgBuffer.kind} header");

            byte[] body = new byte[msgBuffer.bodyLength];

            socket.Receive(body, (int)msgBuffer.bodyLength, SocketFlags.None);
            Console.WriteLine($"{this}: Body of {msgBuffer.kind} received - {msgBuffer.bodyLength}");
            return msgBuffer.BodyToMsg(body);
        }

        public void Send(Msg msg)
        {
            msg.SetNodeId(this.id);

            byte[] buffer = MsgBuffer.MsgToBuffer(msg);
            Console.WriteLine($"{this}: Sending {msg.GetMsgKind()} message - {buffer.Length}");
            socket.Send(buffer); // error handling
        }

        public void Close()
        {
            // TODO: create new Msg Disconnect, to inform other nodes that i'm disconecting

            Console.WriteLine($"{this} ending..");
            this.socket.Close();
        }

        public HelloResponseMsg ListenForHelloResponse()
        {
            // XXX: with timeout & error handling

            var message = this.Receive() as HelloResponseMsg;

            if (message == null)
            {
                throw new Exception("invalid message"); // todo: better exceptions
            }

            return message;
        }  
        
        public HelloMsg ListenForHello()
        {
            // XXX: with timeout & error handling

            var message = this.Receive() as HelloMsg;

            if (message == null)
            {
                throw new Exception("invalid message"); // todo: better exceptions
            }

            return message;
        }

        private void initialize()
        {
            try
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(this.Host, this.Port);
                // todo log
            }
            catch (Exception e)
            {
                // todo log
            }
        }

        public override string ToString()
        {
            return $"Connection: [{Host}:{Port}]";
        }

    }
}
