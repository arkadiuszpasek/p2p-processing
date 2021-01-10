using P2PProcessing.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace P2PProcessingConsole
{
    public enum Level { Debug, Info, Warn, Error };

    public class Log : Logger
    {
        Level level;

        public Log(Level level)
        {
            this.level = level;
        }

        public void Debug(string s)
        {
            if (level <= Level.Debug)
            {
                Console.WriteLine($"[Debug] {s}");
            }
        }

        public void Error(string s)
        {
            if (level <= Level.Error)
            {
                Console.WriteLine($"[Error] {s}");
            }
        }

        public void Info(string s)
        {
            if (level <= Level.Info)
            {
                Console.WriteLine($"[Info] {s}");
            }
        }

        public void Warn(string s)
        {
            if (level <= Level.Warn)
            {
                Console.WriteLine($"[Warn] {s}");
            }
        }
    }
}
