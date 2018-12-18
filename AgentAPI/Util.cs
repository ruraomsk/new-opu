using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace AgentAPI
{
    internal static class Util
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

        static string endString = "\r\n\r\n";
        static string GetAllServerInfo() => "GetAllServerInfo" + endString;
        static string GetDriverInfo(string nameDriver) => "GetDriverInfo" + " " + nameDriver + endString;
        static string CetVariablesInfo() => "CetVariablesInfo" + endString;
        static string GetBlindesInfo() => "GetBlindesInfo" + endString;
        static string GetLoggerInfo() => "GetLoggerInfo" + endString;
        static public object mutex = new object();
        static char[] endChar = { '\n', '\r' };


        private static string ReadResponse(NetworkStream stream)
        {
            byte[] buffer = new byte[32000];
            int cycle = 5;
            while (--cycle > 0)
            {
                string request = "";
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    request += Encoding.UTF8.GetString(buffer, 0, count);
                    if (request.IndexOf(endString) >= 0)
                    {
                        break;
                    }
                }
                if (request.Length == 0)
                {
                    Thread.Sleep(200);
                    continue;
                }
                return request.TrimEnd(endChar);
            }
            return null;
        }

        internal static OPUServer GetServer(NetworkStream stream)
        {
            byte[] data = Encoding.UTF8.GetBytes(GetAllServerInfo());
            stream.Write(data, 0, data.Length);
            string resp = ReadResponse(stream);
            ServerJson sr = JsonConvert.DeserializeObject<ServerJson>(resp);
            OPUServer serv = new OPUServer(sr);
            return serv;
        }
        internal static OPUBlindes GetBlindes(NetworkStream stream)
        {
            byte[] data = Encoding.UTF8.GetBytes(GetBlindesInfo());
            stream.Write(data, 0, data.Length);
            string resp = ReadResponse(stream);
            BlindesJson bd = JsonConvert.DeserializeObject<BlindesJson>(resp);
            OPUBlindes blindes = new OPUBlindes(bd);
            return blindes;
        }

        internal static OPULogs GetLogs(NetworkStream stream)
        {
            byte[] data = Encoding.UTF8.GetBytes(GetLoggerInfo());
            stream.Write(data, 0, data.Length);
            string resp = ReadResponse(stream);
            LogsJson lg = JsonConvert.DeserializeObject<LogsJson>(resp);
            OPULogs lgs = new OPULogs(lg);
            return lgs;
        }

        internal static OPUVariables GetVariables(NetworkStream stream)
        {
            byte[] data = Encoding.UTF8.GetBytes(CetVariablesInfo());
            stream.Write(data, 0, data.Length);
            string resp = ReadResponse(stream);
            VariablesJson vr = JsonConvert.DeserializeObject<VariablesJson>(resp);
            OPUVariables vrs = new OPUVariables(vr);
            return vrs;
        }

        internal static OPUDriver GetDriver(NetworkStream stream, string drv, string type)
        {
            byte[] data = Encoding.UTF8.GetBytes(GetDriverInfo(drv));
            stream.Write(data, 0, data.Length);
            string resp = ReadResponse(stream);
            DriverJson dr = JsonConvert.DeserializeObject<DriverJson>(resp);
            OPUDriver d = new OPUDriver(dr,type);
            return d;
        }
    }
}
