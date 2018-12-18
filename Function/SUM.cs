using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public class SUM : function
    {
        public SUM()
        {
            name = "SUM";
            description = "Сложение всех параматров";
        }
        public override string calculate(List<string> values)
        {
            double sum = 0.0;
            foreach(string s in values)
            {
                sum += double.Parse(s);
            }
            return sum.ToString();
        }
    }
}
