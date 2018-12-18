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
    public static class XMLModbus
    {
        static public Dictionary<string,ModbusRegister> Load(string fileXML)
        {
            Dictionary<string, ModbusRegister> result = new Dictionary<string, ModbusRegister>();
            XmlDocument regXml = new XmlDocument();
            try
            {
                regXml.Load(fileXML);
            }
            catch (Exception err)
            {
                Log.Fatal("XMLApax", err.Message);
                return null;
            }
            foreach (XmlNode n in regXml.SelectNodes("table/records/record"))
            {
                string name = "", description = "", address = "0", ssize = "1", type = "0", format = "2", unitId = "1";
                foreach (XmlNode m in n.ChildNodes)
                {
                    string attr = m.Attributes["name"].Value, attr_txt = m.InnerText;

                    switch (attr) //  В зависимости от типа аттрибута, присваивается значение переменной
                    {
                        case "name":
                            name = attr_txt;
                            break;
                        case "description":
                            description = attr_txt;
                            break;
                        case "address":
                            address = attr_txt;
                            break;
                        case "size":
                            ssize = attr_txt;
                            break;
                        case "type":
                            type = attr_txt;
                            break;
                        case "format":
                            format = attr_txt;
                            break;
                        case "unitId":
                            unitId = attr_txt;
                            break;
                    }
                }
                description = description.Replace('\"', '\'');
                ModbusRegister reg = new ModbusRegister(name, description, int.Parse(type), int.Parse(format), ushort.Parse(address), ushort.Parse(ssize), ushort.Parse(unitId));
                result[name] = reg;
            }
            return result;

        }
    }
}
