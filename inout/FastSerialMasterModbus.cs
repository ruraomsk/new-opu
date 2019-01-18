using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using loggers;


namespace inout
{
    public class ModBusDriverParam
    {
        public string encoding="RTU";     // RTU ASCII 
        public string portname="COM1";     // com1 com2 .....
        public int baudRate=38400;        // 2400 ....
        public int databits=8;        // 5 6 7 8 
        public StopBits stopbits=StopBits.One;        // 1 3 2 
        public Parity parity=Parity.None;          // None=0 Even=2 Odd=1 Mark=3 Space=4

        public ModBusDriverParam()
        {
        }

        public ModBusDriverParam(string portname, string encoding,  int baudRate, int databits, StopBits stopbits, Parity parity)
        {
            this.portname = portname;
            this.encoding = encoding;
            this.baudRate = baudRate;
            this.databits = databits;
            this.stopbits = stopbits;
            this.parity = parity;
        }
    }
    
    public class FastSerialMasterModbus : ModbusCommon
    {
        private SerialPort serialPort = null;
        private ModBusDriverParam param;
        private int timeout;
        private long stepTime;
        private Thread drvThr;
        private const string ClassName = "FastSerialMasterModbus";
        private ConcurrentQueue<ModbusRegisterWithValue> inque;
        private DateTime lastOperation=DateTime.MinValue;
        private ConcurrentDictionary<int, ushort[]> hregs;

        public FastSerialMasterModbus(string name, string description, Dictionary<string, ModbusRegister> regsModbus, ModBusDriverParam param)
        {

            this.name = name;
            this.description = description;
            this.regsModbus = regsModbus;
            this.param = param;
            typeDriver = "MODBUS";
            lastOperation = DateTime.MinValue;
            inque = new ConcurrentQueue<ModbusRegisterWithValue>();
            hregs = new ConcurrentDictionary<int, ushort[]>();
            base.MakeAllArrays();
            if (lenCoils != 0 || lenDis != 0 || lenIrs != 0)
            {
                throw new ArgumentException("Драйвер " + ClassName + " могут быть только holding registers " + name);
            }
            if (lenHrs == 0)
            {
                throw new ArgumentException("Драйвер " + ClassName + " могжет быть только хотя бы один holding register " + name);
            }

            initHregs();

        }
        public override void Init(int step, int timeout)
        {
            this.timeout = timeout;
            stepTime = step;
            Connect = true;
            Reconnect.AddDriver(name, this);
        }


        private void initHregs() {
            for (int i = 1; i < 255; i++)
            {
                hregs[i] = new ushort[lenHrs];
            }
        }


        public override void Start()
        {
            try
            {
                initHregs();

                serialPort = new SerialPort(param.portname);
                serialPort.BaudRate = param.baudRate; // скорость передачи
                serialPort.DataBits = param.databits;
                serialPort.Parity = param.parity;
                serialPort.StopBits = param.stopbits;
                serialPort.ReadTimeout = timeout;
                serialPort.WriteTimeout = timeout;
                serialPort.Open();

                drvThr = new Thread(this.Run);
                drvThr.Start();
                Log.Info(ClassName, "Устройство " + name + " запущено.");
            }
            catch (Exception ex)
            {
                Log.Error(ClassName, "Устройство " + name + " Ошибка запуска " + ex.Message);
                Connect = false;
            }
        }
        public override void Stop()
        {
            if (Connect)
            {
                drvThr.Abort();
                drvThr.Join();
            }    
            if(serialPort!= null)
            {
                serialPort.Dispose();
                serialPort.Close();
                serialPort = null;
            }
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
                        Connect = sendMessage(mregv.register.Uid, mregv.register.Address,mregv.register.SetAsValue(mregv.Value)); 
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(ClassName, "Устройство " + name + " " + ex.Message);
                        Connect = false;
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
            Log.Warn(ClassName, "Устройство " + name + " Завершает работу...");
            Connect = false;
            serialPort.Dispose();
            serialPort.Close();
            serialPort = null;
        }

        private bool sendMessage(int uid, ushort address, ushort[] value)
        {
            try
            {
                byte[] writeBuffer = new byte[1024];
                writeBuffer[0] = (byte)(uid & 0xff);
                writeBuffer[1] = 0x10;
                writeBuffer[2] = (byte)((address >> 8) & 0xff);
                writeBuffer[3] = (byte)(address & 0xff);
                int countBytes = Util.ushort2byte(value, writeBuffer, 7);
                int countRegs = countBytes / 2;
                writeBuffer[4] = (byte)((countRegs >> 8) & 0xff);
                writeBuffer[5] = (byte)(countRegs & 0xff);
                writeBuffer[6] = (byte)(countBytes & 0xff);
                int[] crc = Util.calculateCRC(writeBuffer, 0, countBytes + 7);
                writeBuffer[countBytes + 7] = (byte)(crc[0] & 0xff);
                writeBuffer[countBytes + 8] = (byte)(crc[1] & 0xff);
                serialPort.DiscardInBuffer();
                serialPort.Write(writeBuffer, 0, countBytes + 9);
                Thread.Sleep(25);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(ClassName, "Устройство " + name + " " + ex.Message);
                Connect = false;
                return false;
            }
        }

        public override string GetValue(string nameValue)
        {
            ModbusRegister reg;
            if (!regsModbus.TryGetValue(nameValue, out reg))
            {
                return null;
            }

            lock (mutex)
            {
                return reg.GetAsValue(hregs[reg.Uid]);
            }
        }

        public override bool SetValue(string nameValue, string value)
        {
            ModbusRegister reg;
            if (!regsModbus.TryGetValue(nameValue, out reg))
            {
                return false;
            }

            bool NeedSend = false;
            try
            {
                ushort[] r = reg.SetAsValue(value);
                lock (mutex)
                {
                    ushort[] hhr = hregs[reg.Uid];
                    if (!reg.CompareAsUshort(ref hhr, ref r))
                    {
                        NeedSend = true;
                        for (int i = 0; i < r.Length; i++)
                        {
                            hhr[reg.Address + i] = r[i];
                        }
                        hregs[reg.Uid] = hhr;
                    }
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
