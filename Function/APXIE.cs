using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public class APXIE : function
    {
        public APXIE()
        {
            name = "APXIE";
            description = "Ошибки слотов ввода";
        }

        public override string calculate(List<string> values)
        {
            return Helper.GetAPXIE().ToString();
        }
    }
}
