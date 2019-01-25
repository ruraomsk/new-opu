using System.Collections.Generic;
using System.Linq;

namespace Function
{
    class NOTAND : function
    {
        public NOTAND()
        {
            this.name = "NOTAND";
            this.description = "Функция логическое NOTAND";
        }

        public override string calculate(List<string> values)
        {
            //          bool result = bool.Parse(values[0]);
            bool result = Helper.ReadAsBool(values[0]);
            foreach (string val in values.Skip(1)) {

//                bool b = bool.Parse(val);
                bool b = Helper.ReadAsBool( val );
                result &= b;
            }
            result = !result;
            return result.ToString();
        }
    }
}
