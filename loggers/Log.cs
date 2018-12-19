using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
namespace loggers
{
    public static class Log
    {
        public static bool display = false;
        private static Object mutex=new object();
        private static List<string> Messages = new List<string>();
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static string[] GetLogMessages()
        {
            lock (mutex)
            {
                string[] result = new string[Messages.Count];
                Messages.CopyTo(result);
                return result;
            }
        }
        private static void AddMessage(string module, string message)
        {
            lock (mutex)
            {
                string plainMessage = message.Replace("\"","\'");
                Messages.Add(DateTime.Now.ToLongTimeString() + " - " + module + ": " + message);
                if (Messages.Count > 200) Messages.RemoveAt(0);
            }
        }
        public static void Trace(string module, string message)
        {
            logger.Trace(module + ": " + message);
            AddMessage("Trace " + module, message);
            if (display) Console.WriteLine(module + ": " + message);
        }
        public static void Debug(string module, string message)
        {
            logger.Debug(module + ": " + message);
            AddMessage("Debug " + module, message);
            if (display) Console.WriteLine(module + ": " + message);
        }
        public static void Info(string module, string message)
        {
            logger.Info(module + ": " + message);
            AddMessage("Info " + module, message);
            if (display) Console.WriteLine(module + ": " + message);
        }
        public static void Warn(string module, string message)
        {
            logger.Warn(module + ": " + message);
            AddMessage("WARNING " + module, message);
            if (display) Console.WriteLine(module + ": " + message);
        }
        public static void Error(string module, string message)
        {
            logger.Error(module + ": " + message);
            AddMessage("ERROR " + module, message);
            if (display) Console.WriteLine(module + ": " + message);
        }
        public static void Fatal(string module, string message)
        {
            logger.Fatal(module + ": " + message);
            AddMessage("FATAL " + module, message);
            if (display) Console.WriteLine(module + ": " + message);
        }
    }
}
