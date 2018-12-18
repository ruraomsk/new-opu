using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class DATATIME : function
    {
        public DATATIME()
        {
            name = "DATATIME";
            description = "Преобразование времени в строку";
        }
        public override string calculate(List<string> values)
        {
            return Helper.outTimer(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
        }
    }
}
