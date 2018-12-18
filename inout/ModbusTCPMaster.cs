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
            bool NeedSend = false;
            ModbusRegister reg;
            if (base.TryGetValue(nameValue, out reg))
            {

                try
                {
                    switch (reg.Type)
                    {
                        case 0:
                            bool[] c = reg.SetAsBool(value);
                            lock (mutex)
                            {
                                if (!reg.CompareAsBool(ref coils, ref c))
                                {
                                    NeedSend = true;
                                    for (int i = 0; i < c.Length; i++)
                                    {
                                        coils[reg.Address + i] = c[i];
                                    }
                                }
                            }
                            break;
                        case 3:
                            ushort[] r = reg.SetAsValue(value);
                            lock (mutex)
                            {
                                if (!reg.CompareAsUshort(ref hr, ref r))
                                {
                                    NeedSend = true;
                                    for (int i = 0; i < r.Length; i++)
                                    {
                                        hr[reg.Address + i] = r[i];
                                    }
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn(ClassName, "Устройство " + name + " " + ex.Message);
                    Connect = false;
                }
                if (!Connect)
                {
                    return true;
                }

                if (NeedSend)
                {
                    inque.Enqueue(new ModbusRegisterWithValue(reg, value));
                }

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
                    Log.Info(ClassName, "Устройство " + name + " не запущено." );
                    tcpClient = null;
                    drvThr = null;
                    Connect = false;
                    master = null;
                    return;
                }
                master = ModbusIpMaster.CreateIp(tcpClient);
                master.Transport.ReadTimeout = timeout;
                master.Transport.WriteTimeout = timeout;
                master.Transport.Retries = 3;
                drvThr = new Thread(this.Run);
                drvThr.Start();
                Reconnect.AddDriver(name, this);
                Log.Info(ClassName, "Устройство " + name + " запущено.");

            }
            catch (Exception ex)
            {
                Log.Info(ClassName, "Устройство " + name + " не запущено."+ex.Message);
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
            Log.Info(ClassName, "Устройство " + name + " перезапущенно.");
        }
        public override string Status()
        {
            return "Устройство " + name + ":" + description + " " + (IsConnected() ? "запущено." : "остановлено.")
                    + "Последняя операция " + lastOperation.ToLongTimeString();
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

                    if (!Connect)
                    {
                        continue;
                    }

                    try
                    {
                        switch (mregv.register.Type)
                        {
                            case 0:
                                master.WriteMultipleCoils(mregv.register.Address, mregv.register.SetAsBool(mregv.Value));
                                Connect = true;
                                break;
                            case 3:
                                master.WriteMultipleRegisters(mregv.register.Address, mregv.register.SetAsValue(mregv.Value));
                                Connect = true;
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Warn(ClassName, "Устройство " + name + " " + ex.Message);
                        Connect = false;
                    }
                }
                ReadAllBool(true);
                ReadAllBool(false);
                ReadAllShort(false);
                ReadAllShort(true);
                lastOperation = DateTime.Now;
                long untilTime = (lastOperation.Ticks - tm.Ticks) / 10000L;
                if ((stepTime - untilTime) < 0)
                {
                    Log.Warn(ClassName, "Устройство " + name + " Время цикла превысило шаг и составило " + untilTime.ToString());
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
        private void ReadAllBool(bool coil)
        {
            if (!Connect)
            {
                return;
            }

            if (lenCoils > 0 && coil)
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
                lock (mutex)
                {
                    for (int i = 0; i < lenCoils; i++)
                    {
                        coils[i] = c[i];
                    }
                }
            }
            if (lenDis > 0 && !coil)
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
                lock (mutex)
                {
                    for (int i = 0; i < lenDis; i++)
                    {
                        di[i] = d[i];
                    }
                }
            }
        }
        private void ReadAllShort(bool holden)
        {
            if (!Connect)
            {
                return;
            }

            ushort[] r;
            if (lenIrs > 0 && !holden)
            {
                ushort stadr = 0;
                ushort len = lenIrs;
                do
                {
                    ushort lenl = (len > 100) ? (ushort)100 : len;
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
                return;
            }
            if (lenHrs > 0 && holden)
            {
                ushort stadr = 0;
                ushort len = lenHrs;
                do
                {
                    ushort lenl = (len > 100) ? (ushort)100 : len;
                    try
                    {
                        r = master.ReadHoldingRegisters(stadr, lenl);
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
                            hr[stadr + i] = r[i];
                        }
                    }
                    stadr += lenl;
                    len -= lenl;
                } while (len > 0);
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