using System;
using System.Text;

namespace inout
{
    public class Variable : ViewTable
    {
        public static string DEFAULT_INT = 0.ToString();
        public static string DEFAULT_DOUBLE = 0.0.ToString();
        public static string DEFAULT_BOOL = false.ToString();


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
            string unit = Variable.DEFAULT_DOUBLE;

            switch (TypeVar)
            {
                case Util.TYPEVAR.BOOLEAN:
                    unit = Variable.DEFAULT_BOOL;
                    break;

                case Util.TYPEVAR.INTEGER:
                    unit = Variable.DEFAULT_INT;
                    break;

                case Util.TYPEVAR.DOUBLE:
                    unit = Variable.DEFAULT_DOUBLE;
                    break;
            }

            StringBuilder resultBuilder = new StringBuilder("");

            for (int i=0; i < size; i++) {
                if ( i > 0 ) {
                    resultBuilder.Append (" ");
                }
                resultBuilder.Append(unit);
            }

            return resultBuilder.ToString();
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
            if (TypeVar == Util.TYPEVAR.BOOLEAN)
            {
                SetVarValue(value.ToString());
            }
        }

        public void SetAsInteger(long value)
        {
            if (TypeVar == Util.TYPEVAR.INTEGER)
            {
                SetVarValue(value.ToString());
            }
        }

        public void SetAsDouble(double value)
        {
            if (TypeVar == Util.TYPEVAR.DOUBLE)
            {
                SetVarValue(value.ToString());
            }
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
