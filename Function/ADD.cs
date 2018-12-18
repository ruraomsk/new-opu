using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class ADD : function
    {
        public ADD()
        {
            name = "ADD";
            description = "Сложение с условием";
        }

        public override string calculate(List<string> values)
        {
            int b = int.Parse(values[0]);
            if (bool.Parse(values[1])) b += int.Parse(values[2]);
            return b.ToString();
        }
    }
}
