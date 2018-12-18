using System;
using System.Collections.Generic;

namespace AgentAPI
{
    internal class OPUDriver
    {
        List<Register> regs;
        bool Connect;
        string type;
        public OPUDriver(DriverJson dj, string type)
        {
            this.type = type;
            regs = dj.Values;
            Connect = dj.Connected;
        }

        internal List<string[]> GetValues()
        {
            List<string[]> res = new List<string[]>();
            if (type.Contains("APAX"))
            {
                foreach(Register reg in regs)
                {
                    res.Add( new string[6] { reg.Name, reg.Description, reg.Slot, reg.Address, reg.Size, reg.Value });
                }
                return res;
            };
            if (type.Contains("MODBUS"))
            {
                foreach (Register reg in regs)
                {
                    res.Add(new string[8] { reg.Name, reg.Description, reg.Type,reg.Format, reg.Address, reg.Size, reg.uId,reg.Value });
                }
                return res;
            }
            throw new NotImplementedException();
        }

        internal string[] Columns()
        {
            if (type.Contains("APAX"))
            {
                return new string[6] {"Name","Description","Slot","Address","Size","Value" };
            };
            if (type.Contains("MODBUS"))
            {
                return new string[8] { "Name", "Description", "Type","Format", "Address", "Size","uId", "Value" };
            }
            throw new NotImplementedException();
        }

        internal bool GetConnect()
        {
            return Connect;
        }
    }
    internal class DriverJson
    {
        public string Operation;
        public string Status;
        public bool Connected;
        public List<Register> Values;
    }
    internal class Register
    {
        public string Name;
        public string Description;
        public string Slot;
        public string Address;
        public string Size;
        public string Value;
        public string Type;
        public string Format;
        public string uId;
    }


}