﻿using System;

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
                Console.WriteLine("WARNING! OPU has to have next params: newOPU.exe <left/right> <dir>");
                Console.WriteLine("Now OPU will be use default params");
            }

            string file = (args.Length < 1) ? PathHelper.getDefaultOPUName() : PathHelper.getOPUFileName(args[0]);
            string dir = (args.Length < 2) ? PathHelper.getDefaultPath() : args[1];

            Console.Write("Loading from " + dir + file + " ");
            // Создаем сервер
            ServerOPU server = XMLServer.Load(dir, file);
            Console.Write(".");
            // Установливаем время переподключения
            Reconnect.StartReconnect(server.stepReconnect);
            Console.Write(".");
            // Грузим описания всех устройств
            XMLDevices.Load(dir, file, server);
            Console.Write(".");
            // Грузим описания всех переменных
            XMLVariables.Load(dir, file, server);
            Console.Write(".");
            // Грузим все привязки
            XMLBlinds.Load(dir, file, server);
            Console.Write(".");

            if (server.GetLoadingError())
            {
                Console.Write("\nЕсть ошибки при загрузке функций!\nПрограмма завершена аварийна");
                return;
            }
            // Подключаем слушателя запросов на чтение переменных 
            AgServer agserv = new AgServer(server, 8081);
            agserv.Start();

            Console.Write(".");
            // Запускаем все загруженные и проинициализированные устройства
            server.StartAllDevices();
            Console.Write(".");
            Console.WriteLine("\nAll done.");
            Console.WriteLine("Awaing 5 seconds for looking lamp.");
            Thread.Sleep(5000);
            Console.WriteLine("Server {0} as {1} started...",server.getName() , server.descriptions);

            //            server.PrintVarialble(@"c:\newOPU\var.txt");

            bool Connect = true;
            // все основном цикле считываем все значения с устройств в переменные 
            // делаем один расчет
            // и затем все результаты записываем в устройства
            while (Connect)
            {
                DateTime tm = DateTime.Now;
                lock ( Helper.mainmutex )
                {
                    server.LoadVariablesFromDevices();
                    server.MakeOneStep();
                    server.SaveVariablesToDevices();
                }
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
