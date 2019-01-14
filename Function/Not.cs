using System.Collections.Generic;

namespace Function
{
    class Not : function
    {
        public Not()
        {
            name = "NOT";
            description = "Функция логическое NOT";
        }

        public override string calculate(List<string> values)
        {
            bool result = !Helper.ReadAsBool(values[0]);
            return result.ToString();
        }
    }
}
