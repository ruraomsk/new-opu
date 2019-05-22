using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public class APXOE : function
    {
        public APXOE()
        {
            name = "APXOE";
            description = "Ошибки слотов вывода";
        }

        public override string calculate(List<string> values)
        {
            return "20";
        }
    }
}
