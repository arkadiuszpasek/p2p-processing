using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace P2PProcessing.Problems
{
    [Serializable()]
    [XmlInclude(typeof(Free))]
    [XmlInclude(typeof(Taken))]
    [XmlInclude(typeof(Calculated))]
    public abstract class PayloadState { }

    public class Free : PayloadState
    {
        public int Length;
        public string StartString;

        public static Free Of(int length, string start)
        {
            var obj = new Free();
            obj.Length = length;
            obj.StartString = start;

            return obj;
        }

        public override string ToString()
        {
            return $"State Free, length - {Length}, start - {StartString}";
        }
    }
    public class Taken : PayloadState
    {
        public long Timestamp;
        public int Length;
        public string StartString;

        public static Taken Of(int length, string start)
        {
            var obj = new Taken();
            obj.Length = length;
            obj.StartString = start;
            obj.Timestamp = DateTime.Now.Ticks;

            return obj;
        }
    }
    public class Calculated : PayloadState { }
}
