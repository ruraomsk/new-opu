
using System.Collections.Generic;

namespace Function
{
    class BLINKS : function
    {
        public BLINKS()
        {
            name = "BLINKS";
            description = "Функция AND(S) c мерцанием";
        }

        public override string calculate(List<string> values)
        {
            int a = int.Parse(values[0]);
            int b = int.Parse(values[1]);

            int result = ((a | b) & 9) | ((a & b) & 20);


             return result.ToString();
        }
    }
}
