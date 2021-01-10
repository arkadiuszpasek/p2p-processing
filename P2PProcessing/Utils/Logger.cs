using System;
using System.Collections.Generic;
using System.Text;

namespace P2PProcessing.Utils
{
    public interface Logger
    {
        void Info(string s);
        void Debug(string s);
        void Warn(string s);
        void Error(string s);
    }

    class DefaultLogger : Logger
    {
        public void Debug(string s)
        {
            Console.WriteLine(s);
        }

        public void Error(string s)
        {
            Console.WriteLine(s);
        }

        public void Info(string s)
        {
            Console.WriteLine(s);
        }

        public void Warn(string s)
        {
            Console.WriteLine(s);
        }
    }
}
