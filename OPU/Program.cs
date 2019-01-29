using System;

using builder;
using inout;
using loggers;
using Function;
using System.Threading;
using AgentServer;

namespace OPU
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Error parametrs:usage OPU path_xml_file\n Need OPU <left/right> <dir>");
                Console.WriteLine("Using default params");
            }

            string file = (args.Length < 1) ? PathHelper.getDefaultOPUName() : PathHelper.getOPUFileName(args[0]);
            string dir = (args.Length < 2) ? PathHelper.getDefaultPath() : args[1];

            Console.Write("Loading from " + dir + file + " ");

            ServerOPU server = XMLServer.Load(dir, file);
            Console.Write(".");
            Reconnect.StartReconnect(server.stepReconnect);
            Console.Write(".");
            XMLDevices.Load(dir, file, server);
            Console.Write(".");
            XMLVariables.Load(dir, file, server);
            Console.Write(".");
            XMLBlinds.Load(dir, file, server);
            Console.Write(".");

            if (server.GetLoadingError())
            {
                Console.Write("\nЕсть ошибки при загрузке функций!\nПрограмма завершена аварийна");
                return;
            }

            AgServer agserv = new AgServer(server, 8081);
            Console.Write(".");

            server.StartAllDevices();
            Console.Write(".");
            Console.WriteLine("\nAll done.");
            Console.WriteLine("Awaing 5 seconds for looking lamp.");
            Thread.Sleep(5000);
            Console.WriteLine("Server {0} as {1} started...",server.getName() , server.descriptions);

            server.PrintVarialble();

            bool Connect = true;
            while (Connect)
            {
                DateTime tm = DateTime.Now;

                server.LoadVariablesFromDevices();
                server.MakeOneStep();
                server.SaveVariablesToDevices();
                Helper.blink = !Helper.blink;
                DateTime lastOperation = DateTime.Now;
                long untilTime = (lastOperation.Ticks - tm.Ticks) / 10000L;
                if ((server.stepCycle - untilTime) < 0)
                {
                    Log.Warn("main", "Время цикла превысило шаг и составило " + untilTime.ToString());
                }
                else
                {
                    try
                    {
                        Thread.Sleep((int)(server.stepCycle - untilTime));
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("main", " Выполнение прервано..." + ex.Message);
                        Connect = false;
                    }
                }

            }
            Reconnect.Stop();
            server.StopAllDevices();
            agserv.Stop();
            Console.WriteLine("\nServer stoped.");
        }
    }
}
