using System.Collections.Generic;

namespace Function
{
    class BLINK : function
    {
        public BLINK()
        {
            name = "BLINK";
            description = "Операция AND с мерцанием";
        }

        public override string calculate(List<string> values)
        {
            int inpar = int.Parse(values[0]);
            //bool b = Helper.blink;
            bool b = false;
            if ((inpar & 3) > 0) b = true;
//            else if ((inpar & 4) > 0) b = false;
            return b.ToString();
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
            return (Helper.ReadAsDouble(values[1]) < Helper.ReadAsDouble(values[2])) ? Helper.blink.ToString() : false.ToString();
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
            double p1 = Helper.ReadAsDouble(values[1]);
            float p2 = float.Parse(values[2]);
            return (Helper.ReadAsDouble(values[1]) > Helper.ReadAsDouble(values[1])) ? Helper.blink.ToString() : false.ToString();
        }
    }
}
