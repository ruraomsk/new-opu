using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Function;
namespace inout
{
    public class ApaxRegister : ViewTable
    {
        private string name;
        private string description;
        private int slot;
        private int address;
        private int size;
        public ApaxRegister(string name, string description, int slot, int address, int size)
        {
            this.name = name;
            this.description = description;
            this.slot = slot;
            this.address = address;
            this.size = size;
        }
        public Util.TYPEVAR GetTypeVar()
        {
            return Util.TYPEVAR.BOOLEAN;
        }

        public string GetAsBool(ref bool[] vs)
        {
            StringBuilder builder = new StringBuilder("");
            for (int i = 0; i < size; i++)
            {
                if ( i > 0 ) {
                    builder.Append(" ");
                }
                builder.Append(vs[(address + i) + (slot * Util.MaxChanal)].ToString());
            }
            return builder.ToString();
        }
        public void SetAsBool(ref bool[] vs,string value)
        {
            string[] b = value.Split(' ');
            if(b.Length!=size) new ArgumentException("Не совпадают размеры массивов " + name);
            for (int i = 0; i < size; i++)
            {
                vs[(address + i)+ (slot * Util.MaxChanal)] = Helper.ReadAsBool(b[i]);
            }
        }

        public string Name { get => name; }
        public string Description { get => description; }
        public int Slot { get => slot; }
        public int Address { get => address; }
        public int Size { get => size; }

        public string[] ColumnsName()
        {
            string[] result = { "Name", "Description", "Slot", "Address", "Size" };
            return result;
        }
        public string[] Row(int row)
        {
            string[] result = new string[5];
            result[0] = name;
            result[1] = description;
            result[2] = slot.ToString();
            result[3] = address.ToString();
            result[4] = size.ToString();
            return result;
        }
        public int RowsCount()
        {
            return 1;
        }
    }
    public class ApaxRegisterWithValue
    {
        public ApaxRegister register;
        public String Value;

        public ApaxRegisterWithValue(ApaxRegister register, string value)
        {
            this.register = register;
            Value = value;
        }
    }
}
