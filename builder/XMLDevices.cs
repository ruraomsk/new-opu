using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using inout;
using loggers;

namespace builder
{
    public static class XMLDevices
    {
        static public bool Load(string dir, string file, ServerOPU server)
        {
            XmlDocument regXml = new XmlDocument();
            try
            {
                regXml.Load(dir + file);
            }
            catch (Exception err)
            {
                Log.Fatal("XMLDevices", err.Message);
                return true;
            }
            foreach (XmlNode n in regXml.SelectNodes("Apax/Devices/device"))
            {
                string name = n.Attributes["name"].Value;
                string description = n.Attributes["description"].Value;
                string step = n.Attributes["step"].Value;
                string timeout = n.Attributes["timeout"].Value;
                string type= n.Attributes["type"].Value;

                string load = dir + n.Attributes["load"].Value;

                if (type.Contains("RezCanal"))
                {
                    // DubModbus
                    string[] ips = new string[2];
                    ips[0]= n.Attributes["ip"].Value;
                    ips[1] = n.Attributes["ipdub"].Value;
                    string port= n.Attributes["port"].Value;
                    Dictionary<string, ModbusRegister> regsModbus = XMLModbus.Load(load);
                    DubModbus drv = new DubModbus(name, description, regsModbus, ips, int.Parse(port));
                    server.AddDriver(drv,int.Parse(step),int.Parse(timeout));
                    continue;
                }
                if (type.Contains("ModBusSerialMaster"))
                {
                    // DubModbus
                    string[] ips = new string[2];
                    ModBusDriverParam param = new ModBusDriverParam();
                    param.portname= n.Attributes["portname"].Value;
                    param.encoding = n.Attributes["encoding"].Value;
                    param.baudRate = int.Parse(n.Attributes["baudRate"].Value);
                    Dictionary<string, ModbusRegister> regsModbus = XMLModbus.Load(load);
                    FastSerialMasterModbus drv = new FastSerialMasterModbus(name, description, regsModbus, param);
                    server.AddDriver(drv, int.Parse(step), int.Parse(timeout));
                    continue;
                }
                if (type.Contains("ApaxOutput"))
                {
                    // ApaxOutputDiscret
                    Dictionary<string, ApaxRegister> regsApax = XMLApax.Load(load);
                    ApaxOutputDiscret drv = new ApaxOutputDiscret(name, description, regsApax);
                    server.AddDriver(drv, int.Parse(step), int.Parse(timeout));
                    continue;
                }
                if (type.Contains("ApaxInput"))
                {
                    // ApaxInputDiscret
                    Dictionary<string, ApaxRegister> regsApax = XMLApax.Load(load);
                    ApaxInputDiscret drv = new ApaxInputDiscret(name, description, regsApax);
                    server.AddDriver(drv, int.Parse(step), int.Parse(timeout));
                    continue;
                }
            }
            return true;
        }
    }
}
