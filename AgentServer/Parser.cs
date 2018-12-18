using System;
using System.Collections.Generic;
using inout;
using loggers;

namespace AgentServer
{
    public class Parser
    {
        enum RequestCode
        {
            ErrorRequest,
            GetAllServerInfo,
            GetDriverInfo,
            CetVariablesInfo,
            GetBlindesInfo,
            GetLoggerInfo

        };
        RequestCode requestCode = RequestCode.ErrorRequest;
        string driverName = "";
        private string request;
        private ServerOPU server;
        char[] vs = { '\n', '\r' };
        string result = "{ \"Status\":\"Ok\", ";

        public Parser(string req, ServerOPU server)
        {
            this.request = req.TrimEnd(vs);
            this.server = server;
            string[] parsReq = request.Split(' ');
            if (parsReq[0].Contains("GetAllServerInfo"))
            {
                requestCode = RequestCode.GetAllServerInfo;
            }

            if (parsReq[0].Contains("GetDriverInfo"))
            {
                requestCode = RequestCode.GetDriverInfo;
                driverName = parsReq[1];
            }
            if (parsReq[0].Contains("CetVariablesInfo"))
            {
                requestCode = RequestCode.CetVariablesInfo;
            }

            if (parsReq[0].Contains("GetBlindesInfo"))
            {
                requestCode = RequestCode.GetBlindesInfo;
            }
            if (parsReq[0].Contains("GetLoggerInfo"))
            {
                requestCode = RequestCode.GetLoggerInfo;
            }
        }
        private string makeHeader(bool ok)
        {
            return result = "{ \"Operation\": \""+request+"\", \"Status\":\""+(ok?"Ok":"Error")+"\", ";
        }
        public string GetResponse()
        {
            result = makeHeader(true);
            switch (requestCode)
            {
                case RequestCode.ErrorRequest:
                    return makeHeader(false)+"}";
                case RequestCode.GetAllServerInfo:
                    return workGetAllServerInfo();
                case RequestCode.CetVariablesInfo:
                    return workCetVariablesInfo();
                case RequestCode.GetBlindesInfo:
                    return workGetBlindesInfo();
                case RequestCode.GetDriverInfo:
                    return workGetDriverInfo();
                case RequestCode.GetLoggerInfo:
                    return workGetLoggerInfo();

            }
            return result;
        }

        private string workGetLoggerInfo()
        {
            string[] messages = Log.GetLogMessages();
            result += "\n\"Messages\": [ \n";
            foreach (string item in messages)
            {
                result += "\t\"" + item + "\",\n";
            }
            result += "]\n}\n";
            return result;
        }

        private string workGetDriverInfo()
        {
            ;
            foreach (Driver drv in server.ListAllDevices().Values)
            {
                if (!drv.GetName().Equals(driverName)) continue;
                string[] names = drv.ColumnsName();
                result += "\n\"Connected\": \""+(drv.IsConnected()?"True":"False")+"\", \"Values\": [ \n";
                for (int d= 0; d < drv.RowsCount(); d++)
                {
                    string[] values = drv.Row(d);
                    result += "\t{\n";
                    for (int i = 0; i < names.Length; i++)
                    {
                        result += "\t\t\"" + names[i] + "\":\"" + values[i] + "\",\n";
                    }
                    result += "\t},\n";

                }
                result += "]\n}\n";
                break;
            }
            return result;
        }

        private string workGetBlindesInfo()
        {
            List<string> list= server.ListAllBlaind();
            result += "\n\"Blindes\": [ \n";
            foreach (string blnd in list)
            {
                result += "\t\"" + blnd + "\",\n";
            }
            result += " ]\n}\n";
            return result;
        }

        private string workCetVariablesInfo()
        {
            List<Variable> vars = server.ListAllVariables();
            string[] names = vars[0].ColumnsName();
            result += "\n\"Variables\": [ \n";
            foreach (Variable var in vars)
            {
                string[] values = var.Row(1);
                result += "\t{\n";
                for (int i = 0; i < names.Length; i++)
                {
                    result += "\t\t\"" + names[i] + "\":\"" + values[i] + "\",\n";
                }
                result += "\t},\n";
            }
            result += "]\n} \n";
            return result;
        }
        private string workGetAllServerInfo()
        {
            result += "\"Server\": \""+server.name+"\",";
            result += "\"Description\": \"" + server.descriptions + "\",";
            result += "\"Step\": \"" + server.stepCycle.ToString() + "\",";
            result += "\"Reconnect\": \"" + server.stepReconnect.ToString() + "\",";
            result += "\"Drivers\": [\n";
            foreach (Driver drv in server.ListAllDevices().Values)
            {
                result += "\t{\"Name\": \"" + drv.name + "\", \"Description\": \"" + drv.description + "\",\"Type\": \"" +drv.typeDriver+"\", "
                    +"\"Connected\": \"" + drv.Connect.ToString() + "\"},\n";
            }
            result += "]\n";
            result += "}\n";

            return result;
        }
    }
}
