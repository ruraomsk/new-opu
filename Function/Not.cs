using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class Not : function
    {
        public Not()
        {
            name = "NOT";
            description = "Функция логическое NOT";
        }

        public override string calculate(List<string> values)
        {
            bool b = Helper.ReadAsBool(values[0]);
            b = !b;
            return b.ToString();
        }
    }
}
