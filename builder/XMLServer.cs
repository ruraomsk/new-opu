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
    public static class XMLServer
    {
        static public ServerOPU Load(string fileXML)
        {
            XmlDocument regXml = new XmlDocument();
            try
            {
                regXml.Load(fileXML);
            }
            catch (Exception err)
            {
                Log.Fatal("XMLServer", err.Message);
                return null;
            }
            XmlNode n = regXml.SelectSingleNode("Apax/Server");
            string name = n.Attributes["name"].Value;
            string description= n.Attributes["description"].Value;
            string step = n.Attributes["step"].Value;
            string reconect = n.Attributes["reconect"].Value;
            return new ServerOPU(name, description, int.Parse(step), int.Parse(reconect));
        }
    }
}
