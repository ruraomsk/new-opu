using System;
using System.Globalization;
using System.Text;

namespace Function
{
    static public class Helper
    {
        static public int live = 0;
        static readonly char[] delimiterChars = { '.', ',' };

        static public bool blink = false;
        static public object mainmutex = new object();

        static public int ReadAsInt(string value)
        {
            string[] result = value.Split(delimiterChars);
            return int.Parse(result[0]);
        }

        static public double ReadAsDouble(string value)
        {
            return double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        static public bool ReadAsBool(string value)
        {
            bool result;
            if (bool.TryParse(value, out result)) return result;
            if (value.Equals("0")) return false;
            return true;
        }

        static public bool[] FromStringToBoolArray(string value)
        {
            string[] str = value.Split(' ');
            bool[] result = new bool[str.Length];
            for (int i = 0; i < str.Length; i++) {
                result[i] = bool.Parse(str[i]);
            }
            return result;
        }

        static public int[] FromStringToIntArray(string value)
        {
            string[] str = value.Split(' ');
            int[] result = new int[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                result[i] = int.Parse(str[i]);
            }
            return result;
        }

        static public string outFloat(double infloat, bool blink)
        {
            ushort[] rez;
            byte[] rbyte;
            string sres;

            if (infloat < 0f || infloat > 99999.0f) { sres = "99999"; }
            else if (infloat < 1000.0f) { sres = infloat.ToString("00.000"); }
            else if (infloat < 10000.0f) { sres = infloat.ToString("000.00"); }
            else { sres = infloat.ToString("00000"); }

            rbyte = convertToDisplay(sres);
            rez = upak(rbyte, blink);
            return ArrayToString(rez);
        }

        private static string ArrayToString(ushort[] rez)
        {
            StringBuilder builder = new StringBuilder("");

            foreach (ushort r in rez)
            {
                builder.Append(r.ToString());
                builder.Append(" ");
            }
            builder.Length--;
            return builder.ToString();
        }

        static public string outFloatLong(float infloat, bool blink)
        {
            ushort[] rez;
            byte[] rbyte;
            string sres;

            if (infloat < 0f || infloat > 99999.0f) { sres = "99999"; }
            else if (infloat < 10.0f) { sres = infloat.ToString("0.0000"); }
            else if (infloat < 100.0f) { sres = infloat.ToString("00.000"); }
            else if (infloat < 1000.0f) { sres = infloat.ToString("000.00"); }
            else if (infloat < 10000.0f) { sres = infloat.ToString("0000.0"); }
            else { sres = infloat.ToString("00000"); }

            rbyte = convertToDisplay(sres);
            rez = upak(rbyte, blink);
            return ArrayToString(rez);
        }

        static public string outInteger(int inInt, int len, bool blink)
        {
            ushort[] rez;
            byte[] rbyte;
            string sres = "0";

            switch (len) {
                case 1:
                    sres = inInt.ToString("0");
                    break;
                case 2:
                    sres = inInt.ToString("00");
                    break;

                case 5:
                    sres = inInt.ToString("00000");
                    break;

                default:
                    throw new Exception("Undefined length in outInteger");
            }
            rbyte = convertToDisplay(sres);
            rez = upak(rbyte, blink);
            return ArrayToString(rez);
        }

        static public string outTimer(int thr, int tmm, int tsec)
        {
            ushort[] rez;
            byte[] rbyte;
            string sres = String.Format("{0:00}:{1:00}:{2:00}", thr, tmm, tsec);
            rbyte = convertToDisplay(sres);
            rez = upak(rbyte, false);
            return ArrayToString(rez);
        }


        static private ushort[] upak(byte[] rbyte, bool blink)
        {
            int len = (rbyte.Length % 2 == 0) ? rbyte.Length / 2 : (rbyte.Length / 2) + 1;
            ushort[] rez = new ushort[++len];
            rez[0] = (ushort)(blink ? 0 : 1);
            int j = 1;
            for (int i = 0; i < rbyte.Length; i++)
            {
                rez[j] = (ushort)((i % 2) == 0 ? (rez[j] | (rbyte[i] << 8)) : (rez[j] | (rbyte[i])));
                j += (i % 2) == 0 ? 0 : 1;
            }
            return rez;
        }

        static public byte[] convertToDisplay(string sres)
        {
            bool point = (sres.IndexOf(".") > 0) || (sres.IndexOf(",") > 0);

            byte[] rez = new byte[sres.Length - (point ? 1 : 0)];
            int j = 0;
            for (int i = 0; i < sres.Length; i++)
            {
                byte b = 0xf;
                switch (sres[i])
                {
                    case '0': b = 0; break;
                    case '1': b = 1; break;
                    case '2': b = 2; break;
                    case '3': b = 3; break;
                    case '4': b = 4; break;
                    case '5': b = 5; break;
                    case '6': b = 6; break;
                    case '7': b = 7; break;
                    case '8': b = 8; break;
                    case '9': b = 9; break;
                    case ':': b = 0x2f; break;
                    case ' ': b = 0x7f; break;
                    case ',': b = 0xff; break;
                    case '.': b = 0xff; break;
                }
                if (b != 0xff)
                {
                    rez[j++] = b;
                }
                else rez[j - 1] = (byte)(rez[j - 1] | 0x40);
            }
            return rez;
        }


        //------- APAX Input/Output Errors  ------------------------------------
        private static int MAX_SLOTS = 12;
        private static int SLOT_OVERFLOW = 13;

        private static bool isAPXIE = false;
        private static bool isAPXOE = false;

        private static bool[] slotsErrorsInput = null;
        private static bool[] slotsErrorsOutput = null;

        private static bool[] getBoolArrayWithFalse(int size) {
            bool[] result = new bool[size];
            for (int i = 0; i < size; i++) {
                result[i] = false;
            }
            return result;
        }

        private static ushort GetSlotsErrorsSerialized(bool[] slotsErrors)
        {
            ushort countSlots = (ushort)((slotsErrors.Length <= MAX_SLOTS) ? slotsErrors.Length : SLOT_OVERFLOW);
            ushort result = (ushort)(countSlots << MAX_SLOTS);

            for (int i = 0; i < Math.Min(slotsErrors.Length, MAX_SLOTS); i++)
            {
                ushort bitError = (ushort)((slotsErrors[i] == true) ? 1 : 0);
                bitError <<= i;
                result |= bitError;
            }
            return result;
        }

        public static ushort GetAPXIE() {
            ushort result = (ushort)0;
            if ( isAPXIE ) {
                result = GetSlotsErrorsSerialized(slotsErrorsInput);
            }
            return result;
        }

        public static ushort GetAPXOE() {
            ushort result = 0;
            if ( isAPXOE) {
                result = GetSlotsErrorsSerialized( slotsErrorsOutput );
            }
            return result;
        }

        public static void InitAPXIE(int size) {
            slotsErrorsInput = getBoolArrayWithFalse(size);
            isAPXIE = false;
        }

        public static void InitAPXOE(int size)
        {
            slotsErrorsOutput = getBoolArrayWithFalse(size);
            isAPXOE = false;
        }

        public static void SetSlotErrorInput(int slot) {
            isAPXIE = true;
            if ( slotsErrorsInput != null ) {
                slotsErrorsInput[slot] = true;
            }
        }

        public static void SetSlotErrorOutput(int slot)
        {
            isAPXOE = true;
            if ( slotsErrorsOutput != null) {
                slotsErrorsOutput[slot] = true;
            }
        }
    }
}
