﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using P2PProcessing.Problems;

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
    [XmlRoot("ProblemUpdated")]
    public class ProblemUpdatedMsg : Msg
    {
        public Problem Problem;

        public override MsgKind GetMsgKind()
        {
            return MsgKind.ProblemUpdated;
        }

        public static ProblemUpdatedMsg FromProblem(Problem problem)
        {
            var msg = new ProblemUpdatedMsg();
            msg.Problem = problem;

            return msg;
        }
    }

    [Serializable()]
    [XmlRoot("ProblemSolved")]
    public class ProblemSolvedMsg : Msg
    {
        public Problem Problem;

        public static ProblemSolvedMsg FromResult(string combination, Problem problem)
        {
            var msg = new ProblemSolvedMsg();
            msg.Problem = problem;
            msg.Problem.Solution = combination;

            return msg;
        }
        public override MsgKind GetMsgKind()
        {
            return MsgKind.ProblemSolved;
        }
    }

    public static class Broadcast
    {
        public static byte[] WhoIsPresentMsg = Encoding.UTF8.GetBytes("WhosPresent");
        public static byte[] IAmPresentMsg(int port)
        {
            return Encoding.UTF8.GetBytes($"WhosPresent:{port}");
        }

        public static bool isIAmPresentMsg(byte[] msg)
        {
            var info = Encoding.UTF8.GetString(msg).Split(':');
            if (info.Length == 2 && info[0] == "WhosPresent")
            {
                return true;
            }
            return false;
        }
        public static int parsePresentMsg(byte[] msg)
        {
            return int.Parse(Encoding.UTF8.GetString(msg).Split(':')[1]);
        }
    }
}
