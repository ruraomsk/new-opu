using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class FORIF : function
    {
        public FORIF()
        {
            name = "FORIF";
            description = "Выбор из пары значений одно и расчет средней по всей выборке";
        }

        public override string calculate(List<string> values)
        {
            double b = 0.0;
            int cnt = 0;
            for (int i = 0; i < values.Count; i+=3)
            {
                if (Helper.ReadAsBool(values[i])) b += Helper.ReadAsDouble(values[i + 1]);
                else b += Helper.ReadAsDouble(values[i + 2]);
                cnt++;
            }
            b = b / cnt;
            return b.ToString();
        }
    }
}
