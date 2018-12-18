using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class IFGT : function
    {
        public IFGT()
        {
            name = "IFGT";
            description = "Функция возвращает истина если параметр 1 истина и параметр 2 больше параметра 3";
        }

        public override string calculate(List<string> values)
        {
            return Helper.ReadAsBool(values[0]) && (Helper.ReadAsInt(values[1]) > Helper.ReadAsInt(values[2])) ? true.ToString() : false.ToString();
        }
    }
    class IFLT : function
    {
        public IFLT()
        {
            name = "IFLT";
            description = "Функция возвращает истина если параметр 1 истина и параметр 2 меньше параметра 3";
        }

        public override string calculate(List<string> values)
        {
            return Helper.ReadAsBool(values[0]) && (Helper.ReadAsInt(values[1]) < Helper.ReadAsInt(values[2])) ? true.ToString() : false.ToString();
        }
    }
}
