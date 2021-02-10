using System;
using System.Collections.Generic;
using System.Text;

namespace P2PProcessing.Protocol
{
    public interface Connection
    {
        Msg Receive();
        void Send(Msg msg);

        void Close();

        HelloMsg ListenForHello();
        HelloResponseMsg ListenForHelloResponse();
    }
}
