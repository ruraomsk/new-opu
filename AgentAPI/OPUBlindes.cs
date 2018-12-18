using System;
using System.Collections.Generic;

namespace AgentAPI
{
    internal class OPUBlindes
    {
        List<string> blindes;

        public OPUBlindes(BlindesJson bj)
        {
            blindes = bj.blindes;
        }
        internal List<string> GetAll()
        {
            return blindes;
        }
    }
    internal class BlindesJson
    {
        public string Operation=null;
        public string Status = null;
        public List<string> blindes = null;
    }

}