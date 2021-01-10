using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace P2PProcessing.Protocol
{
    public abstract class Msg
    {
        public Guid nodeId;

        public abstract MsgKind GetMsgKind();

        public Guid GetNodeId()
        {
            return nodeId;
        }

        public void SetNodeId(Guid id)
        {
            this.nodeId = id;
        }
    }

    [Serializable()]
    [XmlRoot("Hello")]
    public class HelloMsg : Msg
    {
        public override MsgKind GetMsgKind()
        {
            return MsgKind.Hello;
        }
    }

    [Serializable()]
    [XmlRoot("HelloResponse")]
    public class HelloResponseMsg : Msg
    {
        public override MsgKind GetMsgKind()
        {
            return MsgKind.HelloResponse;
        }
    }

    [Serializable()]
    [XmlRoot("ProblemPayload")]
    public class ProblemPayloadMsg : Msg
    {
        public override MsgKind GetMsgKind()
        {
            return MsgKind.ProblemPayload;
        }
    }

    [Serializable()]
    [XmlRoot("ProblemResult")]
    public class ProblemResultMsg : Msg
    {
        public override MsgKind GetMsgKind()
        {
            return MsgKind.ProblemResult;
        }
    }
}
