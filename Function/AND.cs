using System.Collections.Generic;

namespace Function
{
    class AND : function
    {
        public AND()
        {
            name = "AND";
            description = "Логическое AND";
        }

        public override string calculate(List<string> values)
        {
            bool b = true;
            foreach(string s in values)
            {
                b &= Helper.ReadAsBool(s);
            }
            return b.ToString();
        }
    }
    class ANDINT : function
    {
        public ANDINT()
        {
            name = "ANDINT";
            description = "Логическое AND над целыми значениями";
        }

        public override string calculate(List<string> values)
        {
            bool b = true;
            int res=0;
            foreach (string s in values)
            {
                if (b)
                {
                    res = int.Parse(s);
                    b = false;
                }
                else {
                    res = res & int.Parse(s);
                }
            }
            return res.ToString();
        }
    }
}
