using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class DIV : function
    {
        public DIV()
        {
            name = "DIV";
            description = "Деление";
        }
        public override string calculate(List<string> values)
        {
            double res=Helper.ReadAsDouble(values[0]) / Helper.ReadAsDouble(values[1]);
            return res.ToString();
        }
    }
}
