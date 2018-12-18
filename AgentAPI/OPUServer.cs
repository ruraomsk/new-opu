using System;
using System.Collections.Generic;

namespace AgentAPI
{
    internal class OPUServer
    {
        public List<Driver> devs;
        public OPUServer(ServerJson sr)
        {
            Name = sr.Server;
            Description = sr.Description;
            Step = sr.Step.ToString();
            Reconnect = sr.Reconnect.ToString();
            devs = sr.Drivers;

        }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string Step { get; internal set; }
        public string Reconnect { get; internal set; }
        
        internal List<string> GetNameDevices()
        {
            List<string> result = new List<string>();
            foreach(Driver dev in devs)
            {
                result.Add(dev.Name);
            }
            return result;
        }

        internal string[] GetColumns()
        {
            return new string[4] { "Name", "Description", "Type", "Connected" };
        }
        internal List<string[]> GetAll()
        {
            List<string[]> result = new List<string[]>();
            foreach (Driver dev in devs)
            {
                result.Add(new string[4] { dev.Name, dev.Description,dev.Type, dev.Connected.ToString() });
            }
            return result;
        }
    }
    internal class ServerJson
    {
        public string Operation;
        public string Status;
        public string Server;
        public string Description;
        public int Step;
        public int Reconnect;
        public List<Driver> Drivers;
    }
    internal class Driver
    {
        public string Name;
        public string Description;
        public string Type;
        public bool Connected;
    }
}