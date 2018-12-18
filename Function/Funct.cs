using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    static public class Funct
    {
        static function[] nameFunction = {
            //new Not(),
            new AND(),
            new OR(),
            //new SUM(),
            new SET(),
            new DATATIME(),
            //new intCONV(),
            //new boolCONV(),
            //new FORMAX(),
            //new FORMAT() ,
            new FORIF(),
            new BLINK(),
            new SWITCH(),
            new FloatStr(),
            new IntStr(),
            new DIV(),
            new ADD(),
            //new BLK(),
            //new EQ(),
            new LIVE(),
            new STATUS(),
            new KEY(),
            //new Mul(),
            new FloatLong(),
            //new NOTAND(),
            new IFLT(),
            new IFGT(),
            new BLINKGT(),
            new BLINKLT(),
            //new BLINKS(),
            new Not()
        };

        public static string DoIt(string namefunc, List<string> param)
        {
            for (int i = 0; i < nameFunction.Length; i++)
            {
                if (nameFunction[i].name.Equals(namefunc)) return nameFunction[i].calculate(param);
            }
            return "0";
        }
        public static bool isPresent(string name)
        {
            for (int i = 0; i < nameFunction.Length; i++)
            {
                if(nameFunction[i].name.Equals(name)) return true;
            }
            return false;
        }

    }
    abstract public class function
    {
        public string name = "";
        public string description = "";
        public abstract string calculate(List<string> values);
    }

}
