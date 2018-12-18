using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class SET : function
    {
        public SET()
        {
            name = "SET";
            description = "Присвоить значение";
        }

        public override string calculate(List<string> values)
        {
            return values[0];    
        }
    }
}
