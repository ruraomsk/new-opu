using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using inout;
using loggers;
using System.Xml;

namespace builder
{
    public static class XMLApax
    {
        static public Dictionary<string,ApaxRegister> Load(string fileXML)
        {
            Dictionary<string, ApaxRegister> result = new Dictionary<string, ApaxRegister>();
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
                string name = "", description = "", address = "0", size = "1", slot = "0";
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
                            size = attr_txt;
                            break;
                        case "slot":
                            slot = attr_txt;
                            break;
                    }
                }
                description=description.Replace('\"', '\'');
                ApaxRegister reg = new ApaxRegister(name, description, int.Parse(slot),  ushort.Parse(address), ushort.Parse(size));
                result[name] = reg;
            }
            return result;
        }
    }
}
