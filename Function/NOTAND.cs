﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            bool result = bool.Parse(values[0]);
            foreach (string val in values.Skip(1)) {
                bool b = bool.Parse(val);
                result = !(result & b);
            }
            return result.ToString();
        }
    }
}
