using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using loggers;
namespace inout
{
    public static class Reconnect 
    {
        private static string ClassName = "Reconnect";
        private static string name = "Reconnect";
        private static string description = "Перезапуск остановленных устройств";
        static private ConcurrentDictionary<string, Driver> drivers = new ConcurrentDictionary<string, Driver>();
        private static int stepTime;
        private static DateTime lastOperation;
        private static Thread drvThr;
        private static bool Connect = true;
        static private Driver[] drvs;
        public static void StartReconnect(int steptime)
        {
            drivers = new ConcurrentDictionary<string, Driver>();
            stepTime = steptime;
            drvThr = new Thread(Run);
            drvThr.Start();
            Log.Info(ClassName, "Процесс " + name + " запущен.");

        }
        public static void Stop()
        {
            drvThr.Abort();
            drvThr.Join();
            Log.Info(ClassName, "Процесс " + name + " остановлен управлением.");
        }
        static public void AddDriver(string name,Driver drv)
        {
            drivers[name] = drv;
            drvs= new Driver[drivers.Count];
            drivers.Values.CopyTo(drvs, 0);
        }
        static public void Run()
        {
            while (Connect)
            {
                DateTime tm = DateTime.Now;
                foreach(Driver drv in drivers.Values)
                {
                    if (!drv.IsConnected())
                    {
                        drv.Reconect();
                    }
                }
                lastOperation = DateTime.Now;
                long untilTime = (lastOperation.Ticks - tm.Ticks) / 10000L;
                if ((stepTime - untilTime) < 0)
                {
                    Log.Warn(ClassName, "Процесс " + name + " Время цикла превысило шаг и составило " + untilTime.ToString());
                }
                else
                {
                    try
                    {
                        Thread.Sleep((int)(stepTime - untilTime));
                    }
                    catch (Exception ex)
                    {

                        Log.Warn(ClassName, "Процесс " + name + " Выполнение прервано..." + ex.Message);
                        Connect = false;
                    }
                }

            }
        }

        public static string[] ColumnsName()
        {
            string[] result = {"Имя","Описание","Состояние" };
            return result;
        }

        public static string[] Row(int row)
        {
            if (row >= drvs.Length) return null;
            string[] result = new string[3];
            result[0] = drvs[row].GetName();
            result[1] = drvs[row].GetDescription();
            result[2] = drvs[row].Status();
            return result;

        }
        public static string Status() => "Процесс " + name + ": " + description + " " + (Connect ? " работает" : " остановлен");
        public static int RowsCount()
        {
            return drvs.Length;
        }
    }
}
