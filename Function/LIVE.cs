using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public class LIVE:function
    {
        public LIVE()
        {
            name = "LIVE";
            description = "Счетчик жизни";
        }

        public override string calculate(List<string> values)
        {
            if (++Helper.live > 32000) Helper.live = 0;
            return Helper.live.ToString();
        }
    }
    public class STATUS : function
    {
        public STATUS()
        {
            name = "STATUS";
            description = "Состояние устройств";
        }

        public override string calculate(List<string> values)
        {
            return "0";
        }
    }
}
