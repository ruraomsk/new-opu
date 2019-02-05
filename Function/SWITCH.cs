using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class SWITCH : function
    {
        public SWITCH()
        {
            name = "SWITCH";
            description = "Вычисление по состоянию переключателя";
        }

        public override string calculate(List<string> values)
        {
            int[] res = new int[5];
            int rez = 0;
            Array.Clear(res, 0, res.Length);
            int k = 4;
            for(int i = 0; i < values.Count; i++)
            {
                if (values.Count==5 && i == 4)
                {
                    res[k--] = bool.Parse(values[i]) ? 1 : 0;
                    continue;
                }
                bool[] b = Helper.FromStringToBoolArray(values[i]);
                res[k--] = position(b);
            }
            rez = res[4] + (res[3] * 10) + (res[2] * 100) + (res[1] * 1000) + (res[0] * 10000);
            if (values.Count == 1) rez++;
            return rez.ToString();
        }

        private int position(bool[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i]) return ++i;
            }
            return 0;
        }
    }
    class KEY : function
    {
        public KEY()
        {
            name = "KEY";
            description = "Обработка ключа запуска";
        }

        public override string calculate(List<string> values)
        {
            SWITCH sw = new SWITCH();
            int rez = int.Parse(sw.calculate(values));
            if (rez == 0) return "2";
            if (rez == 2) return "3";
            return rez.ToString();
        }
    }
}
