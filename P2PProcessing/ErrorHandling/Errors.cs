using System;
using System.Collections.Generic;
using System.Text;

namespace P2PProcessing.ErrorHandling
{
    public class ConnectionException : Exception
    {
        public ConnectionException() : base() { }
        public ConnectionException(string e) : base(e) { }
    }

    public class SessionException : Exception 
    {
        public SessionException() : base() { }
        public SessionException(string e) : base(e) { }
    }

    public class ProtocolException : Exception 
    {
        public ProtocolException() : base() { }
        public ProtocolException(string e) : base(e) { }
    }
    public class ThreadException : Exception 
    {
        public ThreadException() : base() { }
        public ThreadException(string e) : base(e) { }
    }
}
