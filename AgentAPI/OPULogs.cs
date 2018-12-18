using System;
using System.Collections.Generic;

namespace AgentAPI
{
    internal class OPULogs
    {
        List<string> logs ;

        public OPULogs(LogsJson lj)
        {
            logs = lj.Messages;
        }

        internal List<string> GetAll()
        {
            return logs;
        }
    }
    internal class LogsJson
    {
        public string Operation;
        public string Status;
        public List<string> Messages;
    }
}