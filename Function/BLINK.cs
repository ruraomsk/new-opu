using System.Collections.Generic;

namespace Function
{
    class BLINK : function
    {
        public static bool isBlinking(int input) {
            bool result = Helper.blink;

            if ((input & 8) > 0 ) return Helper.blink;
            if ((input & 3) > 0) return true;
            if ((input & 4) > 0) return false;

            return result;
        } 

        public BLINK()
        {
            name = "BLINK";
            description = "Операция AND с мерцанием";
        }

        public override string calculate(List<string> values)
        {
            int inpar = int.Parse(values[0]);



            /*
            //bool b = Helper.blink;
            bool b = false;
            if ((inpar & 3) > 0) b = true;
//            else if ((inpar & 4) > 0) b = false;

    */
            return isBlinking(inpar).ToString();
        }
    }
    class BLINKINT : function
    {
        public BLINKINT()
        {
            name = "BLINKINT";
            description = "Операция AND с мерцанием";
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
                res = res & int.Parse(s);
            }
            res = res & (Helper.blink ? 1 : 0);
            return res.ToString();
        }
    }
    class BLINKLT : function
    {
        public BLINKLT()
        {
            name = "BLINKLT";
            description = "Мерцаем параметр 1 меньше параметра 2";
        }

        public override string calculate(List<string> values)
        {
            bool result = false;

            if (Helper.ReadAsDouble(values[1]) < Helper.ReadAsDouble(values[2])) {
                int inpar = int.Parse(values[0]);
                result = BLINK.isBlinking(inpar);
            }

            //            string blink = Helper.blink.ToString();
            //            string blink = Helper.ReadAsBool(values[0]).ToString();
            //            return (  (Helper.ReadAsDouble(values[1]) < Helper.ReadAsDouble(values[2])  ) ? BLINK.isBlinking(int.Parse(values[0])).ToString() : false.ToString();
            return result.ToString() ;
        }
    }
    class BLINKGT : function
    {
        public BLINKGT()
        {
            name = "BLINKGT";
            description = "Мерцаем параметр 1 больше параметра 2";
        }

        public override string calculate(List<string> values)
        {
            bool result = false;
            if (Helper.ReadAsDouble(values[1]) > Helper.ReadAsDouble(values[2]))
            {
                int inpar = int.Parse(values[0]);
                result = BLINK.isBlinking(inpar);
            }

            /*
            double p1 = Helper.ReadAsDouble(values[1]);
            float p2 = float.Parse(values[2]);
            return (Helper.ReadAsDouble(values[1]) > Helper.ReadAsDouble(values[1])) ? Helper.blink.ToString() : false.ToString();
    */
            return result.ToString();

        }
    }
}
