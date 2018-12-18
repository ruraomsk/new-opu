using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    static public class Helper
    {
        static public int live = 0;

        static public bool blink=false;

        static public int ReadAsInt(string value)
        {
            string[] result = value.Split('.');
            return int.Parse(result[0]);
        }
        static public double ReadAsDouble(string value)
        {
            string[] result = value.Split('.');
            if(result.Length==1) return double.Parse(result[0]+",0");
            return double.Parse(result[0]+","+result[1]);
        }
        static public bool ReadAsBool(string value)
        {
            bool result;
            if (bool.TryParse(value, out result)) return result;
            if (value.Contains("0")) return false;
            return true;
        }

        static public bool[] FromStringToBoolArray(string value)
        {
            string[] str = value.Split(' ');
            bool[] result = new bool[str.Length];
            for(int i = 0; i < str.Length; i++)
            {
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
            if (infloat < 0f || infloat > 99999.0f) sres = "99999";
            else
            { //sres = infloat.ToString("0000.0");
              //
                while (true)
                {
                    if (infloat < 1000.0f) { sres = infloat.ToString("000.00"); break; }
                    if (infloat < 10000.0f) { sres = infloat.ToString("0000.0"); break; }   // исправлено заполнение пробелами
                    sres = infloat.ToString("00000");
                    break;
                }
            }
            rbyte = convertToDisplay(sres);
            rez = upak(rbyte, blink);
            return ArrayToString(rez);
        }

        private static string ArrayToString(ushort[] rez)
        {
            string result = "";
            foreach (ushort r in rez)
            {
                result += r.ToString() + " ";
            }
            return result;
        }

        static public string outFloatLong(float infloat, bool blink)
        {
            ushort[] rez;
            byte[] rbyte;
            string sres;
            if (infloat < 0f || infloat > 99999.0f) sres = "99999";
            else
            { //sres = infloat.ToString("0000.0");
              //
                while (true)
                {
                    if (infloat < 10.0f) { sres = infloat.ToString("0.0000"); break; }
                    if (infloat < 1000.0f) { sres = infloat.ToString("000.00"); break; }
                    if (infloat < 10000.0f) { sres = infloat.ToString("0000.0"); break; }   // исправлено заполнение пробелами
                    sres = infloat.ToString("00000");
                    break;
                }
            }
            rbyte = convertToDisplay(sres);
            rez = upak(rbyte, blink);
            return ArrayToString(rez);
        }
        static public string outInteger(int inInt, int len, bool blink)
        {
            ushort[] rez;
            byte[] rbyte;
            string sres = "0";
            if (len == 5) sres = inInt.ToString("00000");
            if (len == 2) sres = inInt.ToString("00");
            if (len == 1) sres = inInt.ToString("0");
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

    }
}
