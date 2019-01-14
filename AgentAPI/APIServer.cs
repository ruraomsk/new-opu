using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace AgentAPI
{
    public class APIServer
    {
        string ip;
        int port;
        TcpClient client = null;
        NetworkStream stream = null;
        bool Connect = false;
        OPUServer server;
        OPUVariables variables;
        OPUBlindes blindes;
        OPULogs logs;
        Dictionary<string, OPUDriver> drivers=new Dictionary<string, OPUDriver>();
        private int step;
        Thread thread;

        public APIServer(string ip,int port, int step)
        {
            this.ip = ip;
            this.port = port;
            this.step = step;
            try
            {
                client = new TcpClient();
                client.Connect(this.ip, this.port);
                Connect = true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Ошибка сервера " + ip + ":" + port.ToString() + " = " + ex.Message);
            }
            stream = client.GetStream();
            //послать запрос и получить состояние сервера
            server = Util.GetServer(stream );
            //загрузить все формулы
            blindes = Util.GetBlindes(stream);
            // загрузить логи
            logs =Util.GetLogs(stream);
            //загрузить все внутренние переменные
            variables = Util.GetVariables(stream);
            //загрузить все переменные с драйверов
            foreach (Driver drv in server.devs)
            {
                OPUDriver dev = Util.GetDriver(stream, drv.Name,drv.Type);
                drivers.Add(drv.Name, dev);
            }
            if (Connect)
            {
                thread = new Thread(Run);
                thread.Start();
            }
        }
        public void Stop()
        {
            Connect = false;
            thread.Abort();
            thread.Join();
        }
        public void Run()
        {
            while (Connect)
            {
                OPULogs tlog = Util.GetLogs(stream);
                OPUVariables tvar = Util.GetVariables(stream);
                Dictionary<string, OPUDriver> drs = new Dictionary<string, OPUDriver>();
                foreach (Driver drv in server.devs)
                {
                    OPUDriver dev = Util.GetDriver(stream, drv.Name, drv.Type);
                    drs.Add(drv.Name, dev);
                }
                lock (Util.mutex)
                {
                    logs = tlog;
                    variables = tvar;
                    drivers = drs;
                }
                Thread.Sleep(step);
            }
        }
        /*
         *  Возвращает заголовок сервера 
         */
        public string[] ReadServerHead()
        {
            return new string[4] { server.Name, server.Description,server.Step,server.Reconnect };
        }
        /*
         *  Возвращает статус устройства сервера 
         *  подключено или нет
         */
        public bool ReadStatusDevice(string device)
        {
            return drivers[device].GetConnect(); ;
        }
        /*
         *  Возвращает имена всех колонок для перечня устройств 
         */

        public string[] ReadDevicesColumns()
        {
            return server.GetColumns();
        }
        /*
         *  Возвращает заголовок сервера 
         *  Собственно полный список устройств 
         */ 

        public List<string[]> GetAllDevices()
        {
            return server.GetAll();
        }
        /*
         *  Возвращает имена всех колонок переменных для устройства 
         */

        public string[] ReadDeviceColumns(string device)
        {
            return drivers[device].Columns(); ;
        }
        /*
         * Возвращает заголовки переменных системы
         */
        public string[] ReadVariablesColumns()
        {
            return variables.GetColumns();
        }

        /*
         * Возвращает переменные с устройства
         */
        public List<string[]> ReadValuesDevice(string device)
        {
            return drivers[device].GetValues();
        }
        /*
         * Возвращает все формулы системы
         */
        public List<string> ReadAllBlindes()
        {
            return blindes.GetAll();
        }
        /*
         * Возвращает все переменные системы
         */
        public List<string[]> ReadAllVariables()
        {
            return variables.GetAll();
        }
        /*
         * Возвращает весь текущий лог системы
         */
        public List<string> ReadAllLogs()
        {
            return logs.GetAll();
        }

    }
}
