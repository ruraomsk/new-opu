using System;

namespace inout
{
    public class Variable : ViewTable
    {
        private string Name;
        private string Description;
        private string[] ValueVar = new string[3];
        private int Pos;
        private Helper.TYPEVAR TypeVar;
        private bool Changes;

        public Variable(string Name, string Description, Helper.TYPEVAR TypeVar)
        {
            this.Name = Name;
            this.Description = Description;
            this.TypeVar = TypeVar;
            for (Pos = 0; Pos < 3; Pos++)
            {
                ValueVar[Pos] = DefaultValue();
            }
            Changes = true;
            Pos = 0;
        }

        private string DefaultValue()
        {
            switch (TypeVar)
            {
                case Helper.TYPEVAR.BOOLEAN:
                    return "false";
                case Helper.TYPEVAR.INTEGER:
                    return "0";
                case Helper.TYPEVAR.DOUBLE:
                    return "0.0";
            }
            return "0.0";
        }
        public bool IsChanged()
        {
            return Changes;
        }
        public void NewCycle()
        {
            Changes = false;
        }
        public void SetVarValue(string value)
        {
            ValueVar[Pos++] = value;
            Pos = Pos == 3 ? 0 : Pos;
            if (value.CompareTo(GetVarValue()) != 0)
            {
                Changes = true;
            }
        }
        public string GetVarValue()
        {
            if (ValueVar[0].Equals(ValueVar[1]))
            {
                return ValueVar[0];
            }

            if (ValueVar[0].Equals(ValueVar[2]))
            {
                return ValueVar[0];
            }

            if (ValueVar[1].Equals(ValueVar[2]))
            {
                return ValueVar[1];
            }

            return null;
        }
        public string GetVarValueComponent(int index)
        {
            string[] result = GetVarValue().Split(' ');
            if (index < result.Length)
            {
                return result[index];
            }

            return null;
        }
        public bool GetAsBool() => Boolean.Parse(GetVarValue());
        public long GetAsInteger() => long.Parse(GetVarValue());
        public double GetAsDouble() => double.Parse(GetVarValue());
        public void SetAsBool(bool value)
        {
            if (TypeVar != Helper.TYPEVAR.BOOLEAN)
            {
                return;
            }

            SetVarValue(value.ToString());
        }
        public void SetAsInteger(long value)
        {
            if (TypeVar != Helper.TYPEVAR.INTEGER)
            {
                return;
            }

            SetVarValue(value.ToString());
        }
        public void SetAsDouble(double value)
        {
            if (TypeVar != Helper.TYPEVAR.DOUBLE)
            {
                return;
            }

            SetVarValue(value.ToString());
        }
        override public string ToString()
        {
            return Name + "=" + GetVarValue();
        }

        public string[] ColumnsName()
        {
            string[] result = { "Name", "Description", "Type", "Position", "Chanched", "Values" };
            return result;
        }

        public string[] Row(int row)
        {
            string[] result = new string[6];
            result[0] = Name;
            result[1] = Description;
            result[2] = TypeVar.ToString();
            result[3] = Pos.ToString();
            result[4] = Changes.ToString();
            result[5] = "[" + ValueVar[0] + "] [" + ValueVar[1] + "] [" + ValueVar[2] + "]";
            return result;
        }

        public int RowsCount()
        {
            return 1;
        }
        public string GiveDeviceName()
        {
            // если устройство null то это переменная внутренняя
            string[] result = Name.Split(':');
            if (result.Length == 1)
            {
                return null;
            }

            return result[0];
        }
        public string GiveCleanName()
        {
            string[] result = Name.Split(':');
            if (result.Length == 1)
            {
                return Name;
            }

            return result[1];
        }
    }

}
