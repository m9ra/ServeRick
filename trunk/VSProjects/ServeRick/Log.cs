using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick
{
    public static class Log
    {
        public volatile static bool TraceDisabled = true;
        public volatile static bool NoticeDisabled = true;
        private static object _L_output = new object();
        
        public static void Trace(string message, params object[] formatArgs)
        {
            if (TraceDisabled) return;

            var outputMessage = string.Format(message, formatArgs);
            writeline("[Trace] " + outputMessage);
        }
        
        public static void Notice(string message, params object[] formatArgs)
        {
            if (NoticeDisabled) return;

            var outputMessage = string.Format(message, formatArgs);
            writeline("[Notice] " + outputMessage);
        }

        public static void Warning(string message, params object[] formatArgs)
        {
            if (NoticeDisabled) return;

            var outputMessage = string.Format(message, formatArgs);
            writeline("[Warning] " + outputMessage);
        }

        public static void Error(string message, params object[] formatArgs)
        {
            var outputMessage = string.Format(message, formatArgs);
            writeline("[Error] " + outputMessage);
        }

        private static void writeline(string message)
        {
       //     lock (_L_output)
            {
                Console.WriteLine(message);
            }
        }
    }
}
