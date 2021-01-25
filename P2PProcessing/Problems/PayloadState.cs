using System;

namespace P2PProcessing.Problems
{
    abstract class PayloadState { }

    class Free : PayloadState { }
    class Taken : PayloadState
    {
        public long Timestamp;
        public Taken()
        {
            this.Timestamp = DateTime.Now.Ticks;
        }
    }
    class Calculated : PayloadState { }
}
