using P2PProcessing.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace P2PProcessing
{
    class NodeSession
    {
        Session session;
        Connection connection;
        Thread thread;

        public NodeSession(Session session, Connection connection)
        {
            this.session = session;
            this.connection = connection;

            this.thread = new Thread(start);
            thread.Start();
        }

        public void Close()
        {
            if (thread.IsAlive)
            {
                thread.Join();
            }

            connection.Close();
        }

        private void start()
        {
            P2P.logger.Debug($"{this} started");

            try
            {
                while (true)
                {
                    var msg = connection.Receive();
                    session.OnMessage(msg);
                }
            } catch (Exception e)
            {
                this.Close();
            }
        }

        public void Send(Msg msg)
        {
            this.connection.Send(msg);
        }

        public override string ToString()
        {
            return $"Node session - {connection}";
        }
    }
}
