using System;
using System.Collections.Generic;

namespace inout
{
    abstract public class ModbusCommon : Driver, ViewTable
    {
        protected object mutex = new object();
        // Регистры для обмена с устройством
        protected bool[] coils;
        protected bool[] di;
        protected ushort[] ir;
        protected ushort[] hr;
        // Размерности регистров
        protected ushort lenCoils = 0;
        protected ushort lenDis = 0;
        protected ushort lenIrs = 0;
        protected ushort lenHrs = 0;
        protected Dictionary<string, ModbusRegister> regsModbus;

        public override string GetDescription(string nameValue)
        {
            ModbusRegister reg;
            if (!TryGetValue(nameValue, out reg)) return " ";
            return reg.Description;
        }

        public override int GetSize(string nameValue)
        {
            ModbusRegister reg;
            if (!TryGetValue(nameValue, out reg)) return 1;
            return reg.Size;
        }

        protected void MakeAllArrays()
        {
            foreach (ModbusRegister reg in regsModbus.Values)
            {
                if (reg.Type == 0)
                {
                    lenCoils = lenCoils < reg.GetLastPosition() ? reg.GetLastPosition() : lenCoils;
                }

                if (reg.Type == 1)
                {
                    lenDis = lenDis < reg.GetLastPosition() ? reg.GetLastPosition() : lenDis;
                }

                if (reg.Type == 2)
                {
                    lenIrs = lenIrs < reg.GetLastPosition() ? reg.GetLastPosition() : lenIrs;
                }

                if (reg.Type == 3)
                {
                    lenHrs = lenHrs < reg.GetLastPosition() ? reg.GetLastPosition() : lenHrs;
                }
            }
            // Создаем области хранения регистов и копии для доступа
            if (lenCoils > 0)
            {
                coils = new bool[lenCoils];
            }

            if (lenDis > 0)
            {
                di = new bool[lenDis];
            }

            if (lenIrs > 0)
            {
                ir = new ushort[lenIrs];
            }

            if (lenHrs > 0)
            {
                hr = new ushort[lenHrs];
            }
            // Утстанавливаем начальные значения регистров
            for (int i = 0; i < lenCoils; i++)
            {
                coils[i] = false;
            }

            for (int i = 0; i < lenDis; i++)
            {
                di[i] = false;
            }

            for (int i = 0; i < lenIrs; i++)
            {
                ir[i] = 0;
            }

            for (int i = 0; i < lenHrs; i++)
            {
                hr[i] = 0;
            }

        }
        public override Util.TYPEVAR GetTypeVar(string nameValue)
        {
            ModbusRegister reg;
            if (!TryGetValue(nameValue, out reg)) return Util.TYPEVAR.BOOLEAN;
            return reg.GetTypeVar();
        }

        internal bool TryGetValue(string nameValue, out ModbusRegister reg)
        {
            return regsModbus.TryGetValue(nameValue, out reg);
        }

        public override bool IsHaveVariable(string nameValue)
        {
            return regsModbus.ContainsKey(nameValue);
        }

        public override string GetValue(string nameValue)
        {
            ModbusRegister reg;
            if (regsModbus.TryGetValue(nameValue, out reg))
            {
                lock (mutex)
                {
                    switch (reg.Type)
                    {
                        case ModbusRegister.TYPE_COILS:
                            return reg.GetAsBool(coils);

                        case ModbusRegister.TYPE_DI:
                            return reg.GetAsBool(di);

                        case ModbusRegister.TYPE_IR:
                            return reg.GetAsValue(ir);

                        case ModbusRegister.TYPE_HR:
                            return reg.GetAsValue(hr);
                    }
                }
            };
            return null;
        }

    }
}
