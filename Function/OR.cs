using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class OR : function
    {
        public OR()
        {
            name = "OR";
            description = "Логическое OR";
        }

        public override string calculate(List<string> values)
        {
            bool b = false;
            foreach (string s in values)
            {
                b |= Helper.ReadAsBool(s);
            }
            return b.ToString();
        }
    }
    class ORI : function
    {
        public ORI()
        {
            name = "ORI";
            description = "Логическое OR над целыми числами";
        }

        public override string calculate(List<string> values)
        {
            bool b = true;
            int res = 0;
            foreach (string s in values)
            {
                if (b)
                {
                    res = int.Parse(s);
                    b = false;
                }
                res = res | int.Parse(s);
            }
            return res.ToString();
        }
    }
}
