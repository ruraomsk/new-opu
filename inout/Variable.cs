using System;

namespace inout
{
    public class Variable : ViewTable
    {
        private string Name;
        private string Description;
        private string[] ValueVar = new string[3];
        private int Pos;
        private Util.TYPEVAR TypeVar;
        private bool Changes;
        public bool LoadFromDevice = false;
        public bool SaveToDevice = false;


        public Variable(string Name, string Description, Util.TYPEVAR TypeVar, int size=1)
        {
            this.Name = Name;
            this.Description = Description;
            this.TypeVar = TypeVar;

            for (Pos = 0; Pos < 3; Pos++)
            {
                ValueVar[Pos] = DefaultValue(size);
            }
            Changes = true;
            Pos = 0;
        }

        private string DefaultValue(int size)
        {
            string unit = 0.0.ToString();

            switch (TypeVar)
            {
                case Util.TYPEVAR.BOOLEAN:
                    unit = false.ToString();
                    break;

                case Util.TYPEVAR.INTEGER:
                    unit = 0.ToString();
                    break;

                case Util.TYPEVAR.DOUBLE:
                    unit = 0.0.ToString();
                    break;
            }

            string result = "";
            for (int i=0; i < size; i++) {
                if ( i > 0 ) {
                    result += " ";
                }
                result += unit;
            }

            return result;
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
            Pos = ++Pos >= 3 ? 0 : Pos;
            ValueVar[Pos] = value.TrimEnd(' ');
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

            return ValueVar[Pos];
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
            if (TypeVar != Util.TYPEVAR.BOOLEAN)
            {
                return;
            }

            SetVarValue(value.ToString());
        }
        public void SetAsInteger(long value)
        {
            if (TypeVar != Util.TYPEVAR.INTEGER)
            {
                return;
            }

            SetVarValue(value.ToString());
        }
        public void SetAsDouble(double value)
        {
            if (TypeVar != Util.TYPEVAR.DOUBLE)
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
            string[] result = { "Name", "Description", "Type", "Position", "Chanched","From","To", "Values" };
            return result;
        }

        public string[] Row(int row)
        {
            string[] result = new string[8];
            result[0] = Name;
            result[1] = Description;
            result[2] = TypeVar.ToString();
            result[3] = Pos.ToString();
            result[4] = Changes.ToString();
            result[5] = LoadFromDevice.ToString();
            result[6] = SaveToDevice.ToString();
            result[7] = "[" + ValueVar[0] + "] [" + ValueVar[1] + "] [" + ValueVar[2] + "]";
            return result;
        }

        public int RowsCount()
        {
            return 1;
        }
        public string GetName() => Name;
    }

}
