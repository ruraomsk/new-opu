using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace inout
{
    public class DubModbus : ModbusCommon
    {
        private ModbusTCPMaster[] modbuses=null;
        private string[] ips;
        private int port;
        private long stepTime;
        private int timeout;
        private DateTime lastOperation;

        public DubModbus(string name,string description, Dictionary<string, ModbusRegister> regsModbus, string[] ips, int port)
        {
            this.name = name;
            this.description = description;
            this.regsModbus = regsModbus;
            this.ips = ips;
            this.port = port;
            typeDriver = "MODBUS";

            modbuses = new ModbusTCPMaster[ips.Length];
            for(int i=0;i<ips.Length;i++)
            {
                modbuses[i] = new ModbusTCPMaster(name + "_"+i.ToString(), "Канал #" + i.ToString() + " " + description, regsModbus,ips[i],port);
            }
        }

        public override string GetValue(string nameValue)
        {
            lastOperation = DateTime.MinValue;
            int where = -1;
            for (int i = 0; i < modbuses.Length; i++)
            {
                if (modbuses[i].lastOperation > lastOperation)
                {
                    where = i;
                }
            }
            if (where < 0) return "0";
            return modbuses[where].GetValue(nameValue);
        }

        public override bool SetValue(string nameValue, string value)
        {
            bool result = false;
            foreach (ModbusTCPMaster master in modbuses)
            {
                if (!master.IsConnected()) continue;
                result |= master.SetValue(nameValue, value);
            }
            return result;
        }

        public override void Init(int step, int timeout)
        {
            this.stepTime = step;
            this.timeout = timeout;
            foreach(ModbusTCPMaster master in modbuses)
            {
                master.Init(step, timeout);
            }
        }

        public override void Start()
        {
            foreach (ModbusTCPMaster master in modbuses)
            {
                master.Start();
            }
            Connect = true;
        }

        public override void Reconect()
        {
            foreach (ModbusTCPMaster master in modbuses)
            {
                master.Reconect();
            }
        }
        public override void Stop()
        {
            foreach (ModbusTCPMaster master in modbuses)
            {
                master.Stop();
            }
            Connect = false;
        }

        public override string Status()
        {
            return "Устройство " + name + ":" + description + " " + (IsConnected() ? "запущено." : "остановлено.")
        + "Последняя операция " + lastOperation.ToLongTimeString();
        }

        public override string[] ColumnsName()
        {
            return modbuses[0].ColumnsName();
        }

        public override int RowsCount()
        {
            return modbuses[0].RowsCount();
        }

        public override string[] Row(int row)
        {
            lastOperation = DateTime.MinValue;
            int where = -1;
            for (int i = 0; i < modbuses.Length; i++)
            {
                if (modbuses[i].lastOperation > lastOperation)
                {
                    where = i;
                }
            }
            if (where < 0) where = 0; ;
            return modbuses[where].Row(row);
        }
    }
}
