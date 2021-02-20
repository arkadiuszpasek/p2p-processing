using Newtonsoft.Json;
using P2PProcessing.ErrorHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace P2PProcessing.Protocol
{
    public enum MsgKind
    {
        Hello, HelloResponse, ProblemUpdated, ProblemSolved
    }

    class MsgBuffer
    {
        public readonly MsgKind kind;
        public readonly UInt32 bodyLength;

        public MsgBuffer(byte[] header)
        {
            this.kind = determineKindFromHeader(header);
            this.bodyLength = determineBodyLengthFromHeader(header);
        }

        public static byte[] MsgToBuffer(Msg msg)
        {
            try
            {
                byte[] kind = BitConverter.GetBytes((UInt16)msg.GetMsgKind());
                byte[] body;

                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var json = JsonConvert.SerializeObject(msg, settings);

                body = Encoding.UTF8.GetBytes(json);

                byte[] bodyLength = BitConverter.GetBytes((UInt32)body.Length);

                byte[] header = new byte[kind.Length + bodyLength.Length];
                Array.Copy(kind, header, kind.Length);
                Array.Copy(bodyLength, 0, header, 2, bodyLength.Length);

                var buffer = new byte[header.Length + body.Length];

                Array.Copy(header, buffer, header.Length);
                Array.Copy(body, 0, buffer, header.Length, body.Length);

                return buffer;
            } catch (Exception e)
            {
                P2P.logger.Error($"Cannot serialize message: {msg.GetMsgKind()}. {e}");
                throw new ProtocolException($"Cannot serialize message: {msg.GetMsgKind()}");
            }
        }

        public Msg BodyToMsg(byte[] body)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var json = Encoding.UTF8.GetString(body);
                Console.WriteLine(json);
                var msg = JsonConvert.DeserializeObject<Msg>(json, settings);

                if (msg is HelloMsg)
                {
                    return (HelloMsg)msg;
                }
                if (msg is HelloResponseMsg)
                {
                    return (HelloResponseMsg)msg;
                }
                if (msg is ProblemUpdatedMsg)
                {
                    return (ProblemUpdatedMsg)msg;
                }
                if (msg is ProblemSolvedMsg)
                {
                    return (ProblemSolvedMsg)msg;
                }

                throw new ProtocolException("Unknown type");

            }
            catch (Exception e)
            {
                throw new ProtocolException($"Error deserializing message of kind: {kind}. {e}");
            }
        }

        public MsgKind determineKindFromHeader(byte[] header)
        {
            var kind = (Int32)BitConverter.ToUInt16(header, 0);

            if (!Enum.IsDefined(typeof(MsgKind), kind))
            {
                throw new ProtocolException($"Invalid message kind: {kind}");
            }

            return (MsgKind)kind;
        }
        public UInt32 determineBodyLengthFromHeader(byte[] header)
        {
            return BitConverter.ToUInt32(header, 2);
        }

        public static Type getType(MsgKind kind)
        {
            switch (kind)
            {
                case MsgKind.Hello:
                    return typeof(HelloMsg);
                case MsgKind.HelloResponse:
                    return typeof(HelloResponseMsg);
                case MsgKind.ProblemSolved:
                    return typeof(ProblemSolvedMsg);
                case MsgKind.ProblemUpdated:
                    return typeof(ProblemUpdatedMsg);
                default:
                    throw new ProtocolException($"Invalid message kind: {kind}");
            }
        }


        private static XmlSerializer getSerializer(Msg msg)
        {
            return getSerializer(msg.GetType());

        }

        private static XmlSerializer getSerializer(Type type)
        {
            return new XmlSerializer(type);
        }

    }
}
