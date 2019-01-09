using System.Collections.Generic;
using System.Linq;

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
            if (bool.Parse(values[1])) {
                int sum = int.Parse( values[0] );

                foreach (string val in values.Skip(2)) {
                    sum += int.Parse(val);
                }
                return sum.ToString();
            }
            return values[0];
        }
    }
}
