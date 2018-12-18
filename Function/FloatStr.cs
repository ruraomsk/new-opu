using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class FloatStr : function
    {
        public FloatStr()
        {
            name = "FloatStr";
            description = "Преобразование Float в строку";
        }

        public override string calculate(List<string> values)
        {
            return Helper.outFloat(Helper.ReadAsDouble(values[0]), bool.Parse(values[1]));
        }
    }
}
