using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if(args.Length != 1)
            {
                Console.WriteLine("Error parametrs:\nusage OPU path_xml_file");
            }
            string path = args[0];
            Console.Write("Loadin from " + path + " ");
            ServerOPU server = XMLServer.Load(path);
            Console.Write(".");
            Reconnect.StartReconnect(server.stepReconnect);
            Console.Write(".");
            XMLDevices.Load(path, server);
            Console.Write(".");
            XMLVariables.Load(path, server);
            Console.Write(".");
            XMLBlinds.Load(path, server);
            Console.Write(".");
            AgServer agserv = new AgServer(server, 8081);
            Console.Write(".");
            server.StartAllDevices();
            Console.Write(".");
            Console.WriteLine("\nAll done.");
            Console.WriteLine("Awaing 5 seconds for looking lamp.");
            Thread.Sleep(5000);
            Console.WriteLine("Server {0} as {1} started...",server.name,server.descriptions);

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
