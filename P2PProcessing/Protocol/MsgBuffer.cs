using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace P2PProcessing.Protocol
{
    public enum MsgKind
    {
        Hello, HelloResponse, ProblemPayload, ProblemResult
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
            Array.Copy(kind, header, 0);
            Array.Copy(bodyLength, 0, header, 2, bodyLength.Length);

            var buffer = new byte[header.Length + body.Length];

            header.CopyTo(buffer, 0);
            body.CopyTo(buffer, header.Length);

            return buffer;
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
                    throw new Exception(string.Format("Error deserializing message of kind '{0}'", kind), e);
                }
            }
        }

        public MsgKind determineKindFromHeader(byte[] header)
        {
            var typeValue = (Int32)BitConverter.ToUInt16(header, 0);

            if (!Enum.IsDefined(typeof(MsgKind), typeValue))
            {
                throw new Exception(); // TODO: better exceptions
            }

            return (MsgKind)typeValue;
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
                case MsgKind.ProblemPayload:
                    return typeof(ProblemPayloadMsg);
                case MsgKind.ProblemResult:
                    return typeof(ProblemResultMsg);
                default:
                    throw new Exception(); // TODO: better exception
            }
        }


        private static XmlSerializer getSerializer(Msg msg)
        {
            return getSerializer(msg.GetType());

        }

        private static XmlSerializer getSerializer(Type type)
        {
            // xxx: maybe implement reusing of serializers?
            return new XmlSerializer(type);
        }

    }
}
