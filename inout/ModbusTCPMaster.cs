using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using loggers;
using Modbus.Device;
namespace inout
{
    public class ModbusTCPMaster : ModbusCommon
    {
        private const string ClassName = "ModbusTCPMaster";
        private TcpClient tcpClient = null;
        private ModbusIpMaster master = null;
        private Thread drvThr;
        private ConcurrentQueue<ModbusRegisterWithValue> inque;
        private string ip;
        private int port;
        private long stepTime;
        private int timeout;
        public DateTime lastOperation = DateTime.MinValue;

        public ModbusTCPMaster(string name, string description, Dictionary<string, ModbusRegister> regsModbus, string ip, int port)
        {
            this.name = name;
            this.description = description;
            this.regsModbus = regsModbus;
            this.ip = ip;
            this.port = port;
            typeDriver = "MODBUS";
            // Вычисляем размерности регистров
            inque = new ConcurrentQueue<ModbusRegisterWithValue>();
            lastOperation = DateTime.MinValue;
            base.MakeAllArrays();
        }
        public override void Init(int step, int timeout)
        {
            this.timeout = timeout;
            stepTime = step;
        }

        public override bool SetValue(string nameValue, string value)
        {
            ModbusRegister reg;
            if (base.TryGetValue(nameValue, out reg))
            {
                if (!Connect)
                {
                    return false;
                }

                inque.Enqueue(new ModbusRegisterWithValue(reg, value));
                return true;
            };
            return false;
        }

        public override void Start()
        {
            try
            {
                tcpClient = new TcpClient();
                if (!tcpClient.ConnectAsync(ip, port).Wait(1000))
                {
                    throw new Exception("!tcpClient.ConnectAsync error");
                }
                master = ModbusIpMaster.CreateIp(tcpClient);
                master.Transport.ReadTimeout = timeout;
                master.Transport.WriteTimeout = timeout;
                //                master.Transport.Retries = 3;
                master.Transport.Retries = 1;
                drvThr = new Thread(this.Run);


                Connect = true;
                drvThr.Start();
                Reconnect.AddDriver(name, this);

                Log.Info(ClassName, "Устройство " + name + " запущено.");

            }
            catch (Exception ex)
            {
                Log.Info(ClassName, "Устройство " + name + " не запущено." + ex.Message);
                tcpClient = null;
                drvThr = null;
                Connect = false;
                master = null;
            }
        }
        public override void Stop()
        {

            if (drvThr != null)
            {
                drvThr.Abort();
                drvThr.Join();
            }
            if (master != null)
            {
                if (master.Transport != null)
                {
                    master.Transport.Dispose();
                }

                master.Dispose();
            }
            Connect = false;
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient.Dispose();
            }
            Log.Info(ClassName, "Устройство " + name + " остановлено управлением.");
        }
        public override void Reconect()
        {
            Log.Info(ClassName, "Устройство " + name + " перезапускается.");
            Stop();
            Start();
//            Log.Info(ClassName, "Устройство " + name + " перезапущенно.");
        }
        public override string Status()
        {
            return "Устройство " + name + ":" + description + " " + (IsConnected() ? "запущено." : "остановлено.")
                    + "Последняя операция " + lastOperation.ToLongTimeString();
        }

        private bool IsNewCoils(bool[] checkValue, ushort address) {
            for (int i = 0; i < checkValue.Length; i++) {
                if (coils[address + i] != checkValue[i]) {
                    return true;
                }
            }
            return false;
        }

        private bool IsNewHR(ushort[] checkValue, int address) {
            
            for (int i = 0; i < checkValue.Length; i++) {
                if (hr[address + i] != hr[i]) {
                    return true;
                }
            }
            return true;
        }

        override public void Run()
        {
            while (Connect)
            {
                DateTime tm = DateTime.Now;
                while (!inque.IsEmpty)
                {
                    ModbusRegisterWithValue mregv;
                    if (!inque.TryDequeue(out mregv))
                    {
                        break;
                    }

                    try
                    {
                        switch (mregv.register.Type)
                        {
                            case ModbusRegister.TYPE_COILS:
                                // проверка в coil проверить значение
                                bool[] currentValue = mregv.register.SetAsBool(mregv.Value);

                                if (IsNewCoils(currentValue, mregv.register.Address)) {
                                    master.WriteMultipleCoils(mregv.register.Address, currentValue);

                                    for (int i = 0; i < currentValue.Length; i++) {
                                        coils[mregv.register.Address + i] = currentValue[i];
                                    }
                                    Connect = true;
                                }
                                break;

                            case ModbusRegister.TYPE_HR:
                                ushort[] currentValueHR = mregv.register.SetAsValue(mregv.Value);
                                if (IsNewHR(currentValueHR, mregv.register.Address)) {
                                    master.WriteMultipleRegisters(mregv.register.Address, currentValueHR);
                                    for (int i = 0; i < currentValueHR.Length; i++) {
                                        hr[mregv.register.Address + i] = currentValueHR[i];
                                    }
                                    Connect = true;
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(ClassName, "Устройство " + name + " " + ex.Message);
                        Connect = false;
                    }
                }

                long startReadAll = DateTime.Now.Ticks;

                ReadAllCoils();

                long timeEndRealAllCoils = DateTime.Now.Ticks;

                ReadAllDI();
                long timeEndRealAllDI = DateTime.Now.Ticks;

                ReadAllIR();
                long timeEndRealAllIR = DateTime.Now.Ticks;


                ReadBlockHR();

                lastOperation = DateTime.Now;

                long untilTime = (lastOperation.Ticks - tm.Ticks) / 10000L;

                if ((stepTime - untilTime) < 0)
                {

                    Log.Warn(ClassName, "Устройство " + name + " Время цикла превысило шаг и составило " + untilTime.ToString());

                    long timeReadAll = (lastOperation.Ticks - startReadAll) / 10000L;

                    Log.Warn(ClassName, "Время чтения ALL составило " + timeReadAll.ToString());


                    long timeReadAllCoils = (timeEndRealAllCoils - startReadAll) / 10000L;
                    long timeReadAllDI = (timeEndRealAllDI - timeEndRealAllCoils) / 10000L;
                    long timeReadAllIR = (timeEndRealAllIR - timeEndRealAllDI) / 10000L;
                    long timeReadAllHR = (lastOperation.Ticks - timeEndRealAllIR) / 10000L;

                    Log.Warn(ClassName, "Время чтения блоков AllCoils == " + timeReadAllCoils.ToString() + " AllDI == " + timeReadAllDI.ToString() + " AllIR == " + timeReadAllIR.ToString()
                        + " AllHR == " + timeReadAllHR.ToString());
                }
                else
                {
                    try
                    {
                        Thread.Sleep((int)(stepTime - untilTime));
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(ClassName, "Устройство " + name + " Выполнение прервано..." + ex.Message);
                        Connect = false;
                    }
                }
            }

            Log.Warn(ClassName, "Устройство " + name + " Завершает работу...");

            Connect = false;
            master.Transport.Dispose();
            master.Dispose();
            master = null;

            tcpClient.Close();
            tcpClient.Dispose();
            tcpClient = null;
        }

        private void ReadAllCoils() {
            if (Connect && lenCoils > 0)
            {
                bool[] c;
                try
                {
                    c = master.ReadCoils(0, lenCoils);
                }
                catch (Exception ex)
                {
                    Log.Warn(ClassName, "Устройство " + name + " " + ex.Message);
                    Connect = false;
                    return;
                }
                Connect = true;
                //                lock (mutex)
                {
                    for (int i = 0; i < lenCoils; i++)
                    {
                        coils[i] = c[i];
                    }
                }
            }
        }

        private void ReadAllDI() {
            if (Connect && lenDis > 0)
            {
                bool[] d;
                try
                {
                    d = master.ReadInputs(0, lenDis);
                }
                catch (Exception ex)
                {
                    Log.Warn(ClassName, "Устройство " + name + " " + ex.Message);
                    Connect = false;
                    return;
                }
                Connect = true;
                //                lock (mutex)
                {
                    for (int i = 0; i < lenDis; i++)
                    {
                        di[i] = d[i];
                    }
                }
            }
        }


        private void ReadAllIR() {
            if (Connect && lenIrs > 0) {
                ushort[] r;
                ushort stadr = 0;
                ushort len = lenIrs;

                do
                {
                    ushort lenl = (len > ModbusRegister.LENGTH_BLOCK_REGISTERS) ? (ushort)ModbusRegister.LENGTH_BLOCK_REGISTERS : len;
                    try
                    {
                        r = master.ReadInputRegisters(stadr, lenl);
                        Connect = true;
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(ClassName, "Устройство " + name + " " + ex.Message);
                        Connect = false;
                        return;
                    }
                    lock (mutex)
                    {
                        for (int i = 0; i < lenl; i++)
                        {
                            ir[stadr + i] = r[i];
                        }
                    }
                    stadr += lenl;
                    len -= lenl;
                } while (len > 0);
            }
        }

        private void ReadBlockHR() {
            if ( Connect && lenHrs > 0 ) {
                ushort[] r;

                ushort lengthBlock = ( (ushort) (currentLengthHR + ModbusRegister.LENGTH_BLOCK_REGISTERS) <= lenHrs) ? 
                    ModbusRegister.LENGTH_BLOCK_REGISTERS : (ushort)(lenHrs - currentLengthHR) ;

                try {
                    r = master.ReadHoldingRegisters( (ushort)(ModbusRegister.START_HR_ADDRESS + currentLengthHR), lengthBlock);
                    Connect = true;

                }
                catch (Exception ex)
                {
                    Log.Warn(ClassName, "Устройство " + name + " " + ex.Message);
                    Connect = false;
                    return;
                }

                for ( ushort i=0; i< lengthBlock; i++ ) {
                    currentHr[currentLengthHR + i] = r[i];
                }
                currentLengthHR += lengthBlock;

                if (currentLengthHR == lenHrs) {
                    lock (mutex) {
                        for ( int i=0; i < lenHrs; i++) {
                            hr[i] = currentHr[i];
                        }
                    }
                    currentLengthHR = 0;
                }
            }
        }

        public override string[] ColumnsName()
        {
            ModbusRegister[] regs = new ModbusRegister[regsModbus.Count];
            regsModbus.Values.CopyTo(regs, 0);

            string[] result = regs[0].ColumnsName();
            Array.Resize(ref result, result.Length + 1);
            result[result.Length - 1] = "Value";
            return result;
        }

        public override string[] Row(int row)
        {
            ModbusRegister[] regs = new ModbusRegister[regsModbus.Count];
            regsModbus.Values.CopyTo(regs, 0);
            if (row >= regs.Length)
            {
                return null;
            }

            ModbusRegister reg = regs[row];
            string[] result = reg.Row(1);
            Array.Resize(ref result, result.Length + 1);
            lock (mutex)
            {
                switch (reg.Type)
                {
                    case 0:
                        result[result.Length - 1] = reg.GetAsBool(coils);
                        break;
                    case 1:
                        result[result.Length - 1] = reg.GetAsBool(di);
                        break;
                    case 2:
                        result[result.Length - 1] = reg.GetAsValue(ir);
                        break;
                    case 3:
                        result[result.Length - 1] = reg.GetAsValue(hr);
                        break;
                }
            }
            return result;
        }

        public override int RowsCount()
        {
            ModbusRegister[] regs = new ModbusRegister[regsModbus.Count];
            regsModbus.Values.CopyTo(regs, 0);
            return regs.Length;
        }

    }
}