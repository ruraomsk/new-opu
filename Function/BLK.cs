using System.Collections.Generic;

namespace Function
{
    class BLK : function
    {
        public BLK()
        {
            name = "BLK";
            description = "Мерцание левое";
        }

        public override string calculate(List<string> values)
        {
            bool result = false;
            if ( values[0].Equals(values[1]) ) {
                if (bool.Parse(values[2]))
                {
                    result = Helper.blink;
                }
                else {
                    result = true;
                }
            }
            return result.ToString();            
        }
    }
}
