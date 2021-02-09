using System;

namespace P2PProcessing.Problems
{
    public abstract class PayloadState { }

    public class Free : PayloadState
    {
        public int Length;
        public string StartString;

        public Free(int length, string start)
        {
            this.Length = length;
            this.StartString = start;
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
