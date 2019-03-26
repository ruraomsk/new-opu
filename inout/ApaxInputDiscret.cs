using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using loggers;

namespace inout
{
    public class ApaxInputDiscret : ApaxCommon
    {
        private long stepTime;
        private int timeout;
        private Thread drvThr;

        public ApaxInputDiscret(string name, string description, Dictionary<string, ApaxRegister> regsApax)
            :base(name, description, regsApax)
        {
            ClassName = "ApaxInputDiscret";
        }

        public override void Init(int step, int timeout)
        {
            this.timeout = timeout;
            stepTime = step;
            Reconnect.AddDriver(name, this);
        }
        public override void Start()
        {

            Connect = base.Open("5040"); ;
            drvThr = new Thread(this.Run);
            drvThr.Start();
            Log.Info(ClassName, "Устройство " + name + " запущено.");
        }
        public override bool SetValue(string nameValue, string value)
        {
            return false;
        }
        public override void Stop()
        {
            Connect = false;
            drvThr.Abort();
            drvThr.Join();
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
                int index = 0;
                foreach (int slot in slots)
                {
                    bool[] value;
                    if(!adamCtrl.DigitalInput().GetValues(slot,Util.MaxChanal,out value))
                    {
                        Log.Error(ClassName, "Слот " + slot.ToString() + ". Ошибка ввода");
                        continue;
                    }
                    Array.Copy(value, 0,buffer, index, value.Length);
                    index += value.Length;
                }
                lock (mutex)
                {
                    Array.Copy(buffer, varBuffer, buffer.Length);                                
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
            ApaxRegister[] regs = new ApaxRegister[regsApax.Count];
            regsApax.Values.CopyTo(regs, 0);
            string[] result = regs[0].ColumnsName();
            Array.Resize(ref result, result.Length + 1);
            result[result.Length - 1] = "Value";
            return result;
        }

        public override string[] Row(int row)
        {
            ApaxRegister[] regs = new ApaxRegister[regsApax.Count];
            regsApax.Values.CopyTo(regs, 0);
            if (row >= regs.Length)
            {
                return null;
            }

            ApaxRegister reg = regs[row];
            string[] result = reg.Row(1);
            Array.Resize(ref result, result.Length + 1);
            lock (mutex)
            {
                result[result.Length - 1] = reg.GetAsBool(ref varBuffer);
            }
            return result;
        }

        public override int RowsCount()
        {
            ApaxRegister[] regs = new ApaxRegister[regsApax.Count];
            regsApax.Values.CopyTo(regs, 0);
            return regs.Length;
        }

    }
}
