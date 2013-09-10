using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer
{
    static class Log
    {
        internal volatile static bool TraceDisabled = true;
        internal volatile static bool NoticeDisabled = true;
        private static object _L_output = new object();


        internal static void Trace(string message, params object[] formatArgs)
        {
            if (TraceDisabled) return;

            var outputMessage = string.Format(message, formatArgs);
            writeline("[Trace] " + outputMessage);
        }


        internal static void Notice(string message, params object[] formatArgs)
        {
            if (NoticeDisabled) return;

            var outputMessage = string.Format(message, formatArgs);
            writeline("[Notice] " + outputMessage);
        }

        internal static void Error(string message, params object[] formatArgs)
        {
            var outputMessage = string.Format(message, formatArgs);
            writeline("[Error] " + outputMessage);
        }

        private static void writeline(string message)
        {
            lock (_L_output)
            {
                Console.WriteLine(message);
            }
        }
    }
}
