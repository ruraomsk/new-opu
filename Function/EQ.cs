using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class EQ : function
    {
        public EQ()
        {
            this.name = "EQ";
            this.description = "Сравнение";
        }

        public override string calculate(List<string> values)
        {
            bool result = ( values[0].Equals(values[1]) ) ? true : false;
            return result.ToString();
        }
    }
}
