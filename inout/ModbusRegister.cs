using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inout
{
    public class ModbusRegister :ViewTable
    {
        public const int TYPE_COILS=0;
        public const int TYPE_DI =1;
        public const int TYPE_IR=2;
        public const int TYPE_HR=3;

        public const ushort LENGTH_BLOCK_REGISTERS = 125;

        public const ushort START_HR_ADDRESS = 0;

        private string name;
        private string description;
        private int type;
        private int format;
        private int address;
        private int size;
        private int uid;

        public ModbusRegister(string name, string description, int type, int format, int address, int size, int uid)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.description = description ?? throw new ArgumentNullException(nameof(description));
            this.type = type;
            this.format = format;
            this.address = address;
            this.size = size;
            this.uid = uid;
        }

        public string Name { get => name;  }
        public string Description { get => description; }
        public int Type { get => type; }
        public int Format { get => format; }
        public ushort Address { get => (ushort)address; }
        public int Size { get => size; }
        public int Uid { get => uid; }

        public String getStringId { get => uid.ToString() + ":" + address.ToString(); }

        public Util.TYPEVAR GetTypeVar()
        {
            if (type < 2) return Util.TYPEVAR.BOOLEAN;
            switch (format)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    return Util.TYPEVAR.INTEGER;
                case 8:
                case 9:
                    return Util.TYPEVAR.DOUBLE;
                case 11:
                case 13:
                    return Util.TYPEVAR.INTEGER;
                case 14:
                case 15:
                    return Util.TYPEVAR.DOUBLE;
            }
            return Util.TYPEVAR.BOOLEAN;
        }

        public int GetElementLenght()
        {
            if (type < 2) return 1;
            int len = 1;
            if (format >= 4 && format <= 9) len = 2;
            if (format >= 11 && format <= 15) len = 4;
            return len;
        }
        public int GetFullLenght()
        {
            return GetElementLenght() * size;
        }
        public ushort GetLastPosition()
        {
            return (ushort)(address + GetFullLenght());
        }
        public bool CompareAsBool(ref bool[] buffer,ref bool[] bufferValue)
        {
            if(address+ bufferValue.Length> buffer.Length) return false;
            for (int i=0;i< bufferValue.Length; i++)
            {
                if (buffer[address + i] != bufferValue[i]) return false;
            }
            return true;
        }

        public bool CompareAsUshort(ref ushort[] buffer, ref ushort[] bufferValue)
        {
             if (address + bufferValue.Length > buffer.Length) return false;
            for (int i = 0; i < bufferValue.Length; i++)
            {
                if (buffer[address + i] != bufferValue[i]) return false;
            }
            return true;
        }

        public string[] ColumnsName()
        {
            string[] result = { "Name", "Description", "Type", "Format", "Address","Size", "uId" };
            return result;
        }

        public string[] Row(int row)
        {
            string[] result = new string[7];
            result[0] = name;
            result[1] = description;
            result[2] = type.ToString();
            result[3] = format.ToString();
            result[4] = address.ToString();
            result[5] = size.ToString();
            result[6] = uid.ToString();
            return result;
        }
        public int RowsCount()
        {
            return 1;
        }

        public string GetAsBool(bool[] vs)
        {
            if(type > 1) throw new ArgumentException("Неверный тип " + name);
            string result = "";
            for(int i=0;i<size;i++) result+=vs[address+i].ToString()+" ";
            return result;
        }

        public bool[] SetAsBool(string value)
        {
            if (type > 1) throw new ArgumentException("Неверный тип " + name);
            string[] result = value.Split(' ');
            bool[] b = new bool[size];
            for (int i = 0; i < size; i++)
            {
                b[i] = bool.Parse(result[i]);
            };
            return b;
        }

        public string GetAsValue(ushort[] vs)
        {
            if (GetLastPosition() > vs.Length) throw new ArgumentException("Большой адрес " + name);
            if (type < 2) throw new ArgumentException("Неверный тип " + name);

            StringBuilder resultBuilder = new StringBuilder("");

            
            for (int i = 0; i < size; i++)
            {
                if (i > 0) {
                    resultBuilder.Append(" ");
                }
                switch (format)
                {
                    case 2:
                    case 3:
                        // двух байтный целый
                        resultBuilder.Append(vs[address + i].ToString());
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        resultBuilder.Append( BitConverter.ToInt32(Read4Bytes(vs,i), 0).ToString());
                        break;
                    case 8:
                    case 9:
                        resultBuilder.Append( BitConverter.ToSingle(Read4Bytes(vs,i), 0).ToString() );
                        break;
                    case 11:
                    case 13:
                        resultBuilder.Append( BitConverter.ToInt64(Read8Bytes(vs,i), 0).ToString());
                        break;
                    case 14:
                    case 15:
                        resultBuilder.Append(BitConverter.ToDouble(Read8Bytes(vs,i), 0).ToString());
                        break;
                }
            };

            return resultBuilder.ToString();
        }
        public ushort[] SetAsValue(string value)
        {
            if (type < 2) throw new ArgumentException("Большой адрес " + name);
            string[] result = value.Split(' ');
            ushort[] b = new ushort[GetFullLenght()];

            for (int i = 0; i < size; i++)
            {
                switch (format)
                {
                    case 2:
                    case 3:
                        // двух байтный целый
                        ushort[] j1 = new ushort[1];
                        j1[0] = BitConverter.ToUInt16(BitConverter.GetBytes(int.Parse(result[i])), 0);
                        AddArray(ref b,j1,i);
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        AddArray(ref b, Write4BatesInt(result[i]), i);
                        break;
                    case 8:
                    case 9:
                        AddArray(ref b, Write4BatesFloat(result[i]), i);
                        break;
                    case 11:
                    case 13:
                        AddArray(ref b, Write8BatesInt(result[i]), i);
                        break;
                    case 14:
                    case 15:
                        AddArray(ref b, Write8BatesFloat(result[i]), i);
                        break;
                }
            }
            return b;
        }

        private void AddArray(ref ushort[] b, ushort[] j,int count)
        {
            for(int i = 0; i < j.Length; i++)
            {
                b[i + (count * GetElementLenght())] = j[i];
            }
        }

        private byte[] Read4Bytes(ushort[] vs,int i)
        {
            byte[] b1 = BitConverter.GetBytes(vs[address+(i*GetElementLenght())]);
            byte[] b2 = BitConverter.GetBytes(vs[address + 1 + (i * GetElementLenght())]);
            byte[] bb = new byte[4];
            bb[0] = b2[0];
            bb[1] = b2[1];
            bb[2] = b1[0];
            bb[3] = b1[1];
            return bb;
        }

        private byte[] Read8Bytes(ushort[] vs, int i)
        {
            byte[] b1 = BitConverter.GetBytes(vs[address + (i * GetElementLenght())]);
            byte[] b2 = BitConverter.GetBytes(vs[address + 1 + (i * GetElementLenght())]);
            byte[] b3 = BitConverter.GetBytes(vs[address + 2 + (i * GetElementLenght())]);
            byte[] b4 = BitConverter.GetBytes(vs[address + 3 + (i * GetElementLenght())]);
            byte[] bb = new byte[8];
            bb[0] = b4[0];
            bb[1] = b4[1];
            bb[2] = b3[0];
            bb[3] = b3[1];
            bb[4] = b2[0];
            bb[5] = b2[1];
            bb[6] = b1[0];
            bb[7] = b1[1];
            return bb;
        }
        private ushort[] Write4BatesInt(string value)
        {
            // четырех байтный целый
            ushort[] j = new ushort[2];
            j[1] = BitConverter.ToUInt16(BitConverter.GetBytes(Int32.Parse(value)), 0);
            j[0] = BitConverter.ToUInt16(BitConverter.GetBytes(Int32.Parse(value)), 2);
            return j;
        }
        private ushort[] Write4BatesFloat(string value)
        {
            // четырех байтный float
            ushort[] j = new ushort[2];
            j[1] = BitConverter.ToUInt16(BitConverter.GetBytes(float.Parse(value)), 0);
            j[0] = BitConverter.ToUInt16(BitConverter.GetBytes(float.Parse(value)), 2);
            return j;
        }
        private ushort[] Write8BatesFloat(string value)
        {
            // 8 байтный целый
            ushort[] j = new ushort[4];
            j[3] = BitConverter.ToUInt16(BitConverter.GetBytes(Int64.Parse(value)), 0);
            j[2] = BitConverter.ToUInt16(BitConverter.GetBytes(Int64.Parse(value)), 2);
            j[1] = BitConverter.ToUInt16(BitConverter.GetBytes(Int64.Parse(value)), 4);
            j[0] = BitConverter.ToUInt16(BitConverter.GetBytes(Int64.Parse(value)), 6);
            return j;
        }

        private ushort[] Write8BatesInt(string value)
        {
            // 8 байтный float
            ushort[] j = new ushort[4];
            j[3] = BitConverter.ToUInt16(BitConverter.GetBytes(double.Parse(value)), 0);
            j[2] = BitConverter.ToUInt16(BitConverter.GetBytes(double.Parse(value)), 2);
            j[1] = BitConverter.ToUInt16(BitConverter.GetBytes(double.Parse(value)), 4);
            j[0] = BitConverter.ToUInt16(BitConverter.GetBytes(double.Parse(value)), 6);
            return j;
        }
    }

    public class ModbusRegisterWithValue
    {
        public ModbusRegister register;
        public String Value;

        public ModbusRegisterWithValue(ModbusRegister register, string value)
        {
            this.register = register;
            this.Value = value;
        }
    }
}
