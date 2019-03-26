using loggers;
using Modbus.Device;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace inout
{
    public class ModbusTCPSlave : ModbusCommon
    {
        private const string ClassName = "ModbusTCPSlave";
        private int port;
        private DateTime lastOperation = DateTime.MinValue;
        private long stepTime;
        private int timeout;
        public ModbusSlave slave;
        private TcpListener slaveTcpListener;
        private Thread drvThr;
        private ConcurrentQueue<ModbusRegisterWithValue> inque;

        public ModbusTCPSlave(string name, string description, Dictionary<string, ModbusRegister> regsModbus, int port)
        {
            this.name = name;
            this.description = description;
            this.regsModbus = regsModbus;
            this.port = port;
            lastOperation = DateTime.MinValue;
            typeDriver = "MODBUS";
            inque = new ConcurrentQueue<ModbusRegisterWithValue>();
            base.MakeAllArrays();
        }
        public override void Init(int step, int timeout)
        {
            this.timeout = timeout;
            stepTime = step;
            Reconnect.AddDriver(name, this);
        }
        public override void Start()
        {
            //IPAddress addr = IPAddress.Parse("0.0.0.0");
            slaveTcpListener = new TcpListener(IPAddress.Any, port);
            slaveTcpListener.Start();
            //slaveTcpListener.Server.Blocking=false;
            slave = ModbusTcpSlave.CreateTcp(1, slaveTcpListener);
            //            slave.Transport.ReadTimeout = timeout;
            //            slave.Transport.WriteTimeout = timeout;
            slave.DataStore = Modbus.Data.DataStoreFactory.CreateDefaultDataStore(lenCoils, lenDis, lenHrs, lenIrs);
            slave.Listen();
            Connect = true;
            drvThr = new Thread(this.Run);
            drvThr.Start();
            Log.Info(ClassName, "Устройство " + name + " запущено.");
        }
        public override bool SetValue(string nameValue, string value)
        {
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
                                for (int i = 0; i < c.Length; i++)
                                {
                                    coils[reg.Address + i] = c[i];
                                }
                            }
                            break;
                        case 1:
                            bool[] d = reg.SetAsBool(value);
                            lock (mutex)
                            {
                                for (int i = 0; i < d.Length; i++)
                                {
                                    di[reg.Address + i] = d[i];
                                }
                            }
                            break;
                        case 2:
                            ushort[] ri = reg.SetAsValue(value);
                            lock (mutex)
                            {
                                for (int i = 0; i < ri.Length; i++)
                                {
                                    ir[reg.Address + i] = ri[i];
                                }
                            }
                            break;
                        case 3:
                            ushort[] r = reg.SetAsValue(value);
                            lock (mutex)
                            {
                                for (int i = 0; i < r.Length; i++)
                                {
                                    hr[reg.Address + i] = r[i];
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
                inque.Enqueue(new ModbusRegisterWithValue(reg, value));
                return true;
            };
            return false;
        }
        public override void Stop()
        {
            drvThr.Abort();
            drvThr.Join();
            slave.Transport.Dispose();
            slave.Dispose();
            Connect = false;
            slaveTcpListener.Stop();
            //slaveTcpListener.Server.Disconnect(true);
            Log.Info(ClassName, "Устройство " + name + " остановлено управлением.");
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
                                bool[] c = mregv.register.SetAsBool(mregv.Value);
                                lock (mutex)
                                {
                                    for (int i = 0; i < c.Length; i++)
                                    {
                                        slave.DataStore.CoilDiscretes[mregv.register.Address + i + 1] = c[i];
                                    }

                                }
                                Connect = true;
                                break;
                            case 1:
                                bool[] d = mregv.register.SetAsBool(mregv.Value);
                                lock (mutex)
                                {
                                    for (int i = 0; i < d.Length; i++)
                                    {
                                        slave.DataStore.InputDiscretes[mregv.register.Address + i + 1] = d[i];
                                    }
                                }
                                Connect = true;
                                break;
                            case 2:
                                ushort[] ri = mregv.register.SetAsValue(mregv.Value);
                                lock (mutex)
                                {
                                    for (int i = 0; i < ri.Length; i++)
                                    {
                                        slave.DataStore.InputRegisters[mregv.register.Address + i + 1] = ri[i];
                                    }
                                }
                                Connect = true;
                                break;
                            case 3:
                                ushort[] h = mregv.register.SetAsValue(mregv.Value);
                                lock (mutex)
                                {
                                    for (int i = 0; i < h.Length; i++)
                                    {
                                        slave.DataStore.HoldingRegisters[mregv.register.Address + i + 1] = h[i];
                                    }
                                }
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
                lock (mutex)
                {
                    for (int i = 0; i < coils.Length; i++)
                    {
                        coils[i] = slave.DataStore.CoilDiscretes[i + 1];
                    }
                    for (int i = 0; i < di.Length; i++)
                    {
                        di[i] = slave.DataStore.InputDiscretes[i + 1];
                    }
                    for (int i = 0; i < ir.Length; i++)
                    {
                        ir[i] = slave.DataStore.InputRegisters[i + 1];
                    }
                    for (int i = 0; i < hr.Length; i++)
                    {
                        hr[i] = slave.DataStore.HoldingRegisters[i + 1];
                    }
                }
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