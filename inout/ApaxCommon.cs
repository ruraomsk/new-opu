using System;
using System.Collections.Generic;
using Advantech.Adam;

using loggers;

namespace inout
{
    public class ApaxCommon : Driver, ViewTable
    {
        protected object mutex = new object();
        protected Dictionary<string, ApaxRegister> regsApax;
        protected bool[] varBuffer;
        protected bool[] buffer;
        protected AdamControl adamCtrl;
        protected int[] slots;
        protected string[] nameDevices;

        protected DateTime lastOperation;

        protected void MakeAllArrays()
        {
            varBuffer = new bool[Util.MaxDevice * Util.MaxChanal];
            buffer = new bool[Util.MaxDevice * Util.MaxChanal];
        }

        public ApaxCommon(string name, string description, Dictionary<string, ApaxRegister> regsApax) {
            // input:
            this.name = name;
            this.description = description;
            this.regsApax = regsApax;
            lastOperation = DateTime.MinValue;
            typeDriver = "APAX";
            //inque = new ConcurrentQueue<ApaxRegisterWithValue>();
            MakeAllArrays();

            /*
                         this.name = name;
            this.description = description;
            this.regsApax = regsApax;
            lastOperation = DateTime.MinValue;
            typeDriver = "APAX";
            //inque = new ConcurrentQueue<ApaxRegisterWithValue>();
            base.MakeAllArrays();

             */
        }

        public override string GetDescription(string nameValue)
        {
            ApaxRegister reg;
            if (!TryGetValue(nameValue, out reg)) return " ";
            return reg.Description;
        }
        public bool Open(string nameDevice)
        {
            try
            {
                adamCtrl = new AdamControl(AdamType.Apax5000);
                if (!adamCtrl.OpenDevice())
                {
                    Log.Error("ApaxDeriverOpen", "Не могу открыть шину APAX");
                    return false;
                }
                if (!adamCtrl.Configuration().GetSlotInfo(out nameDevices))
                {
                    Log.Error("ApaxDeriverOpen", "Не могу прочитать список устройств");
                    return false;
                }
                int count = 0;
                foreach (string nd in nameDevices)
                {
                    if (nameDevice.Equals(nd))
                    {
                        count++;
                    }
                }
                if (count == 0)
                {
                    Log.Error("ApaxDeriverOpen", "Нет устройств " + nameDevice + " на шине APAX");
                    return false;
                }
                slots = new int[count];
                count = 0;
                for (int i = 0; i < nameDevices.Length; i++)
                {
                    if (nameDevice.Equals(nameDevices[i]))
                    {
                        slots[count++] = i;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal("ApaxDeriverOpen", "Глобальная ошибка! " + ex.Message);
                return false;
            }
        }
        public override Util.TYPEVAR GetTypeVar(string nameValue)
        {
            ApaxRegister reg;
            if (!TryGetValue(nameValue, out reg)) return Util.TYPEVAR.BOOLEAN;
            return reg.GetTypeVar();
        }
        internal bool TryGetValue(string nameValue, out ApaxRegister reg)
        {
            return regsApax.TryGetValue(nameValue, out reg);
        }
        public override bool IsHaveVariable(string nameValue)
        {
            return regsApax.ContainsKey(nameValue);
        }
        public override string GetValue(string nameValue)
        {
            ApaxRegister reg;
            if (!regsApax.TryGetValue(nameValue, out reg))
            {
                return null;
            }
            lock (mutex)
            {
                return reg.GetAsBool(ref varBuffer);
            }
        }
    }
}
