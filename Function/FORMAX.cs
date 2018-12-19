using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    class FORMAX : function
    {
        public FORMAX()
        {
            this.name = "FORMAX";
            this.description = "Выбор максимального";
        }

        public override string calculate(List<string> values)
        {
            float maxValue = float.Parse(values[0]);

            foreach ( string val in values.Skip(1) ) {
                float current = float.Parse(val);
                if ( current > maxValue ) {
                    maxValue = current;
                }
            }

            return maxValue.ToString() ;
        }
    }
}
