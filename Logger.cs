using System;
using System.IO;
using System.Linq;

namespace CashFlowBot
{
    public static class Logger
    {
        private static string LogDir => $"{AppDomain.CurrentDomain.BaseDirectory}/Logs";
        public static string LogFile => $"{LogDir}/{DateTime.Today:yyyy-MM-dd}.txt";

        static Logger() => Directory.CreateDirectory(LogDir);

        public static string Top => string.Join(Environment.NewLine, File.ReadAllLines(LogFile).Reverse().Take(30).Reverse());

        public static void Log(Exception exception)
        {
            Log(exception.Message);
            Log(exception.StackTrace);
        }

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogFile, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch { /*nothing*/ }
        }
    }
}
