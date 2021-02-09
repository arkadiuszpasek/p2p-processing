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
        Hello = 1, HelloResponse, ProblemUpdated, ProblemSolved
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

                XmlSerializer serializer = getSerializer(msg);

                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, msg);

                    body = stream.ToArray();
                }

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
            var serializer = getSerializer(getType(kind));

            using (var stream = new MemoryStream(body, 0, body.Length, false))
            {
                try
                {
                    return (Msg)serializer.Deserialize(stream);
                }
                catch (Exception e)
                {
                    throw new ProtocolException($"Error deserializing message of kind: {kind}. {e}");
                }
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
