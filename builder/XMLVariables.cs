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
    public static class XMLVariables
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
                Log.Fatal("XMLVariables", err.Message);
                return false;
            }
            foreach (XmlNode n in regXml.SelectNodes("Apax/Variables/var"))
            {
                addVariable(server,n);
            }
            foreach (XmlNode n in regXml.SelectNodes("Apax/Constants/var"))
            {
                addVariable(server, n);
            }
            return true;
        }

        private static void addVariable(ServerOPU server, XmlNode n)
        {
            string name = n.Attributes["name"].Value;
            string description = n.Attributes["description"].Value;
            string type = n.Attributes["type"].Value;
            string value = n.Attributes["value"].Value;
            Util.TYPEVAR ttype = Util.TYPEVAR.DOUBLE;
            if (type.Contains("bool")) ttype = Util.TYPEVAR.BOOLEAN;
            if (type.Contains("float")) ttype = Util.TYPEVAR.DOUBLE;
            if (type.Contains("int")) ttype = Util.TYPEVAR.INTEGER;
            description = description.Replace('\"', '\'');
            Variable var = new Variable(name, description, ttype);
            var.SetVarValue(value);
            var.SetVarValue(value);
            var.SetVarValue(value);
            server.AddVariable(var);
        }
    }
}
