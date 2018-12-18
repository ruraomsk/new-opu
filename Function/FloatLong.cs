using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class FloatLong : function
    {
        public FloatLong()
        {
            name = "FloatLong";
            description = "Float в строку 4 знака";
        }

        public override string calculate(List<string> values)
        {
            return Helper.outFloatLong(float.Parse(values[0]),bool.Parse(values[1]));
        }
    }
}
