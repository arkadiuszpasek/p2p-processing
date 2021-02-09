using System;
using System.Xml.Serialization;

namespace P2PProcessing.Problems
{
    public abstract class PayloadState { }

    [Serializable()]
    [XmlRoot("Free")]
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
        public Taken()
        {
            this.Timestamp = DateTime.Now.Ticks;
        }
    }
    public class Calculated : PayloadState { }
}
