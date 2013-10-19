using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer;

namespace TestWebApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Expects path to root folder of web");
                return;
            }

            var webApp = new TestWeb(args[0]);
            ServerEnvironment.AddApplication(webApp);
            ServerEnvironment.Start();

            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();

                switch (keyInfo.KeyChar)
                {
                    case 't':
                        Log.TraceDisabled = !Log.TraceDisabled;
                        break;
                    case 'n':
                        Log.NoticeDisabled = !Log.NoticeDisabled;
                        break;
                }

            } while (keyInfo.Key != ConsoleKey.Escape);
            Environment.Exit(0);   
        }
    }
}
