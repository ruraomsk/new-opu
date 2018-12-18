using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inout
{
    public class Blind
    {
        public string function;
        public string resultName;
        public List<string> paramNames;

        public Blind(string function)
        {
            this.function = function;
            paramNames = new List<string>();
        }
        public void AddResultName(string name)
        {
            resultName = name;
        }
        public void AddParametr(string name)
        {
            paramNames.Add(name);
        }
        public override string ToString()
        {
            string result= resultName+"="+function+"(";
            bool flag = false;
            foreach(string p in paramNames)
            {
                result += (flag ? "," : "") + p;
                flag = true;
            }
            result += ");";
            return result;
        }
    }
}
