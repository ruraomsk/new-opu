using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class IntStr : function
    {
        public IntStr()
        {
            name = "IntStr";
            description = "Преобразование Integer в строку";
        }

        public override string calculate(List<string> values)
        {
            return Helper.outInteger(Helper.ReadAsInt(values[0]), Helper.ReadAsInt(values[0]), Helper.ReadAsBool(values[1]));
        }
    }
}
