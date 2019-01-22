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
    public static class XMLBlinds
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
                Log.Fatal("XMLBlinds", err.Message);
                return true;
            }
            foreach (XmlNode n in regXml.SelectNodes("Apax/Blinds"))
            {
                string load = dir+n.Attributes["load"].Value;
                XMLBlindFile.Load( load, server);
            }
            return true;
        }
    }

    public static class XMLBlindFile
    {
        static public bool Load(string fileXML, ServerOPU server)
        {
            XmlDocument regXml = new XmlDocument();
            try
            {
                regXml.Load(fileXML);
            }
            catch (Exception err)
            {
                Log.Fatal("XMLBlind", err.Message);
                return true;
            }
            foreach (XmlNode n in regXml.SelectNodes("Blinds/blind"))
            {
                string namefunction = n.Attributes["name"].Value;
                Blind blnd = new Blind(namefunction);
                foreach (XmlNode x in n.ChildNodes)
                {
                    string namevar= x.Attributes["name"].Value;
                    string type=x.Attributes["type"].Value;
                    if (type.Contains("up"))
                    {
                        blnd.AddParametr(namevar);
                    };
                    if (type.Contains("out"))
                    {
                        blnd.AddResultName(namevar);
                    }
                }
                server.AddBlind(blnd);
            }
            return true;
        }
    }

}
