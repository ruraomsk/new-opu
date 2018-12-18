using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class intCONV : function
    {
        public intCONV()
        {
            name = "iCONV";
            description = "Перевод из битового массива в число";
        }

        public override string calculate(List<string> values)
        {
            bool[] b = Helper.FromStringToBoolArray(values[0]);
            for(int i = 0; i < b.Length; i++)
            {
                if (b[i]) return i.ToString();
            }
            return "0";
        }
    }
}
