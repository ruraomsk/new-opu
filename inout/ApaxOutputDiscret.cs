﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using loggers;

namespace inout
{
    public class ApaxOutputDiscret : ApaxCommon
    {
        private DateTime lastOperation = DateTime.MinValue;
        private long stepTime;
        private int timeout;
        private Thread drvThr;
        //private ConcurrentQueue<ApaxRegisterWithValue> inque;
        private const string ClassName = "ApaxOutputDiscret";

        public ApaxOutputDiscret(string name, string description, Dictionary<string, ApaxRegister> regsApax)
        {
            this.name = name;
            this.description = description;
            this.regsApax = regsApax;
            lastOperation = DateTime.MinValue;
            typeDriver = "APAX";
            //inque = new ConcurrentQueue<ApaxRegisterWithValue>();
            base.MakeAllArrays();

            for (int i = 0; i < varBuffer.Length; i++) {
                varBuffer[i] = true;
            }
        }
        public override void Init(int step, int timeout)
        {
            this.timeout = timeout;
            stepTime = step;
            Reconnect.AddDriver(name, this);
        }
        public override void Start()
        {

            Connect = base.Open("5046"); ;
            drvThr = new Thread(this.Run);
            drvThr.Start();
            Log.Info(ClassName, "Устройство " + name + " запущено.");
        }
        public override bool SetValue(string nameValue, string value)
        {
            ApaxRegister reg;
            if (!base.TryGetValue(nameValue, out reg))
            {
                return false;
            }
            lock (mutex)
            {
                reg.SetAsBool(ref varBuffer, value);
            }
            return true;
        }
        public override void Stop()
        {
            Connect = false;
            drvThr.Abort();
            drvThr.Join();
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
                lock (mutex)
                {
                    Array.Copy(varBuffer, buffer, buffer.Length);
                }
                int index = 0;
                bool[] value = new bool[Util.MaxChanal];
                foreach (int slot in slots)
                {
                    Array.Copy(buffer, index, value, 0, value.Length);
                    index += value.Length;
                    if (!adamCtrl.DigitalOutput().SetValues(slot, value))
                    {
                        Log.Error(ClassName, "Слот " + slot.ToString() + "Ошибка вывода");
                        continue;
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
