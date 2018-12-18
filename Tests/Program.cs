using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using builder;
using inout;
using loggers;
using Function;
using System.Threading;
using AgentAPI;
using NLog;
using AgentServer;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
           // Logger logger = LogManager.GetCurrentClassLogger();
            TestControl t = new TestControl();
            t.AllTests();

            Console.WriteLine("Tests done");
            Console.ReadKey();
        }
    }
    class TestControl
    {
        public void AllTests()
        {
            bool AllOk = true;
            //AllOk |= TestLogger();
            //AllOk |= TestVariable();
            //AllOk |= TestVariable();
            //AllOk |= TestModbusRegister();
            //AllOk |= TestApaxRegister();
            //TestXMLApax();
            //TestXMLModbus();
            //AllOk |= TestModbusTCP();
            //AllOk |= TestFastSerial();
            //AllOk |= TestApaxOutput();
            //AllOk |= TestApaxInput();
            //AllOk |= TestLoadServer();
            //AllOk |= TestServerAgentParser();
            AllOk |= TestServerAgentFull();

        }

        private bool TestServerAgentFull()
        {
            Console.Write("Testing Server Agent and Client\n");
            ServerOPU serverL = XMLServer.Load("c:/OPU/OPULeft.xml");
            XMLDevices.Load("c:/OPU/OPULeft.xml", serverL);
            XMLVariables.Load("c:/OPU/OPULeft.xml", serverL);
            XMLBlinds.Load("c:/OPU/OPULeft.xml", serverL);
            Reconnect.StartReconnect(serverL.stepReconnect);

            AgServer agserv = new AgServer(serverL, 8081);
            Log.Debug("Test", "Server and agent started");

            APIServer agent = new APIServer("127.0.0.1", 8081,1000);
            Thread.Sleep(10000);

            //PrintArray (agent.ReadServerHead());
            //PrintArray(agent.ReadDevicesColumns());
            //PrintList(agent.GetAllDevices());
            //PrintStringList(agent.ReadAllBlindes());
            //PrintStringList(agent.ReadAllLogs());
            //PrintList(agent.ReadAllVariables());
            PrintArray(agent.ReadDeviceColumns("Baz1"));
            PrintList(agent.ReadValuesDevice("Baz1"));
            PrintArray(agent.ReadDeviceColumns("DI5040"));
            PrintList(agent.ReadValuesDevice("DI5040"));

            agent.Stop();
            agserv.Stop();
            return true;
        }

        private void PrintStringList(List<string> list)
        {
            foreach (string item in list)
            {
                Console.WriteLine(item);
            };
        }

        private void PrintList(List<string[]> list)
        {
            foreach (string[] item in list)
            {
                PrintArray(item);
            };
        }

        private void PrintArray(string[] ar)
        {
            for (int i = 0; i < ar.Length; i++)
            {
                Console.Write(ar[i] + "\t");
            }
            Console.Write("\n");
        }
        private bool TestServerAgentParser()
        {
            string endString = "\r\n\r\n";
            Parser par;
            string reqtest;
            Console.Write("Testing Server Agent\n");
            ServerOPU serverL = XMLServer.Load("c:/OPU/OPULeft.xml");
            XMLDevices.Load("c:/OPU/OPULeft.xml", serverL);
            XMLVariables.Load("c:/OPU/OPULeft.xml", serverL);
            XMLBlinds.Load("c:/OPU/OPULeft.xml", serverL);
            reqtest= "GetAllServerInfo "+endString;
            par = new Parser(reqtest, serverL);
            Console.WriteLine(par.GetResponse());
            Console.WriteLine("Step done");
            Console.ReadKey();

            reqtest = "CetVariablesInfo" + endString;
            par = new Parser(reqtest, serverL);
            Console.WriteLine(par.GetResponse());
            Console.WriteLine("Step done");
            Console.ReadKey();
            reqtest = "GetLoggerInfo" + endString;
            par = new Parser(reqtest, serverL);
            Console.WriteLine(par.GetResponse());
            Console.WriteLine("Step done");
            Console.ReadKey();

            reqtest = "GetDriverInfo BarsDU" + endString;
            par = new Parser(reqtest, serverL);
            Console.WriteLine(par.GetResponse());
            Console.WriteLine("Step done");
            Console.ReadKey();
            reqtest = "GetDriverInfo DO5046" + endString;
            par = new Parser(reqtest, serverL);
            Console.WriteLine(par.GetResponse());
            Console.WriteLine("Step done");
            Console.ReadKey();
            reqtest = "GetBlindesInfo" + endString;
            par = new Parser(reqtest, serverL);
            Console.WriteLine(par.GetResponse());
            Console.WriteLine("Step done");
            Console.ReadKey();

            return true;
        }

        private bool TestLoadServer()
        {
            Console.Write("Testing Load Server\n");
            ServerOPU serverL = XMLServer.Load("c:/OPU/OPULeft.xml");
            XMLDevices.Load("c:/OPU/OPULeft.xml", serverL);
            XMLVariables.Load("c:/OPU/OPULeft.xml", serverL);
            XMLBlinds.Load("c:/OPU/OPULeft.xml", serverL);
            Reconnect.StartReconnect(serverL.stepReconnect);
            //serverL.StartAllDevices();
            for (int i = 0; i < 100; i++)
            {
                serverL.LoadVariablesFromDevices();
                serverL.MakeOneStep();
                serverL.SaveVariablesToDevices();
                Helper.blink = !Helper.blink;
            }
            //serverL.StopAllDevices();
            foreach (Variable var in serverL.ListAllVariables())
            {
                string[] res = var.Row(1);
                foreach (string str in res)
                {
                    Console.Write(str + "\t");

                }
                Console.Write("\n");
            }
            Reconnect.Stop();

            //foreach (string st in serverL.ListAllBlaind())
            //{
            //    Console.WriteLine(st);

            //}
            ServerOPU serverR = XMLServer.Load("c:/OPU/OPURight.xml");
            XMLDevices.Load("c:/OPU/OPULeft.xml", serverR);
            XMLVariables.Load("c:/OPU/OPULeft.xml", serverR);
            XMLBlinds.Load("c:/OPU/OPULeft.xml", serverR);
            Reconnect.StartReconnect(serverR.stepReconnect);
            for (int i = 0; i < 100; i++)
            {
                serverR.LoadVariablesFromDevices();
                serverR.MakeOneStep();
                serverR.SaveVariablesToDevices();
                Helper.blink = !Helper.blink;
            }
            //serverL.StopAllDevices();
            foreach (Variable var in serverR.ListAllVariables())
            {
                string[] res = var.Row(1);
                foreach (string str in res)
                {
                    Console.Write(str + "\t");

                }
                Console.Write("\n");
            }
            Reconnect.Stop();
            return true;
        }

        private bool TestApaxOutput()
        {
            Console.Write("Testing class ApaxOutput\n");
            Dictionary<string, ApaxRegister> regsApax = XMLApax.Load(".../../../test/LampL.xml");
            ApaxOutputDiscret master = new ApaxOutputDiscret("APAXDO", "Вывод лампочек", regsApax);
            Reconnect.StartReconnect(10000);
            master.Init(500, 1000);
            master.Start();
            for (int i = 0; i < 5; i++)
            {
                master.SetValue("L2_3L", "true");
                master.SetValue("LSETLA", "true");
                Thread.Sleep(1000);
                Console.WriteLine(master.GetValue("L2_3L") + "\t" +
                master.GetValue("LSETLA") + "\t");
            }
            for (int i = 0; i < Reconnect.RowsCount(); i++)
            {
                string[] r = Reconnect.Row(i);
                Console.WriteLine(r[0] + "\t" + r[1] + "\t" + r[2]);
            }
            Reconnect.Stop();
            master.Stop();
            Console.WriteLine(" ok done");
            return true;
        }
        private bool TestApaxInput()
        {
            Console.Write("Testing class ApaxInput\n");
            Dictionary<string, ApaxRegister> regsApax = XMLApax.Load(".../../../test/ButtonL.xml");
            ApaxInputDiscret master = new ApaxInputDiscret("APAXIO", "Ввод кнопочек и тумблеров", regsApax);
            Reconnect.StartReconnect(10000);
            master.Init(500, 1000);
            master.Start();
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine("{"+master.GetValue("F3_2_2L") + "}\t" +
                master.GetValue("B18_5L") + "\t");
            }
            for (int i = 0; i < Reconnect.RowsCount(); i++)
            {
                string[] r = Reconnect.Row(i);
                Console.WriteLine(r[0] + "\t" + r[1] + "\t" + r[2]);
            }
            Reconnect.Stop();
            master.Stop();
            Console.WriteLine(" ok done");
            return true;
        }

        public bool TestVariable()
        {

            Variable varbool = new Variable("TESTVARBOOL", "Проверка переменной bool", Util.TYPEVAR.BOOLEAN);
            Variable varint = new Variable("TESTVARINT", "Проверка переменной int", Util.TYPEVAR.INTEGER);
            Variable varflt = new Variable("TESTVARFLT", "Проверка переменной float", Util.TYPEVAR.DOUBLE);
            Console.Write("Testing class Variable ");

            varbool.SetVarValue("true");
            varbool.SetVarValue("true");
            if (!varbool.GetAsBool())
            {
                Console.WriteLine("Bool is bad ");
                return false;
            }
            varbool.SetVarValue("true false");
            varbool.SetVarValue("true false");
            if (bool.Parse(varbool.GetVarValueComponent(1)))
            {
                Console.WriteLine("Bool is bad ");
                return false;
            }

            varflt.SetAsDouble(123.4567890);
            varflt.SetAsDouble(123.4567890);
            if (varflt.GetAsDouble() != 123.4567890)
            {
                Console.WriteLine("Dooble is bad ");
                return false;
            }
            Console.WriteLine(" ok done");
            return true;
        }
        public bool TestModbusRegister()
        {
            ModbusRegister rbool = new ModbusRegister("RBOOL", "REGISTER BOOL", 0, 2, 0, 3, 1);
            ModbusRegister rsint = new ModbusRegister("RSINT", "REGISTER SINT", 3, 2, 0, 1, 1);
            ModbusRegister rint = new ModbusRegister("RINT", "REGISTER INT", 3, 4, 0, 1, 1);
            ModbusRegister rfloat = new ModbusRegister("RFLOAT", "REGISTER FLOAT", 3, 8, 0, 1, 1);
            ModbusRegister rlong = new ModbusRegister("RLONG", "REGISTER LONG", 3, 11, 0, 1, 1);
            ModbusRegister rdouble = new ModbusRegister("RDOUBLE", "REGISTER DOUBLE", 3, 14, 0, 1, 1);
            Console.Write("Testing class ModbusRegister ");
            bool[] b = rbool.SetAsBool("True False True");
            string s = rbool.GetAsBool(b);
            if (rbool.GetAsBool(b).CompareTo("True False True ")!=0)
            {
                Console.WriteLine("Convert bool is bad ");
                return false;
            }
            if (rsint.GetAsValue(rsint.SetAsValue("1234")).CompareTo("1234 ") != 0)
            {
                Console.WriteLine("Convert small int is bad ");
                return false;
            }

            Console.WriteLine(" ok done");
            return true;
        }
        private bool TestLogger()
        {
            string nameLog = "loggerTest";
            Console.Write("Testing class loggers ");
            Log.Debug(nameLog, "It's debug");
            Log.Info(nameLog, "It's info");
            Log.Warn(nameLog, "It's warning");
            Log.Trace(nameLog, "It's trace message");
            Log.Error(nameLog, "It's error");
            Log.Fatal(nameLog, "It's fatal message");

            Console.WriteLine(" ok done");
            return true;
        }
        private bool TestFastSerial()
        {
            Console.Write("Testing classe FastSerialMasterModbus\n");
            Dictionary<string, ModbusRegister> regsModbus = XMLModbus.Load(".../../../test/FoutL.xml");
            //            ModbusTCPMaster master = new ModbusTCPMaster("master","Тестовый мастер",regsModbus,"127.0.0.1",502);
            FastSerialMasterModbus master = new FastSerialMasterModbus("indic", "Цифровые индикаторы", regsModbus, new ModBusDriverParam());
            Reconnect.StartReconnect(10000);
            master.Init(500, 1000);
            master.Start();
            for (int i = 0; i < 5; i++)
            {
                master.SetValue("FD1", Helper.outFloat(float.Parse("123,34"), false));
                master.SetValue("FD2", Helper.outInteger(int.Parse("334"), 5,false));
                Thread.Sleep(1000);
                Console.WriteLine(master.GetValue("FD1") + "\t" +
                master.GetValue("FD2") + "\t");
            }
            for (int i = 0; i < Reconnect.RowsCount(); i++)
            {
                string[] r = Reconnect.Row(i);
                Console.WriteLine(r[0] + "\t" + r[1] + "\t" + r[2]);
            }
            Reconnect.Stop();
            master.Stop();
            Console.WriteLine(" ok done");
            return true;
        }

        private bool TestModbusTCP()
        {
            Console.Write("Testing classes ModbusTCP Master and Slave\n");
            Dictionary<string, ModbusRegister> regsModbus = XMLModbus.Load(".../../../test/du.xml");
            //            ModbusTCPMaster master = new ModbusTCPMaster("master","Тестовый мастер",regsModbus,"127.0.0.1",502);
            string[] ips = { "127.0.0.1", "127.0.0.1", "127.0.0.1" };
            DubModbus master = new DubModbus("master", "Тестовый мастер", regsModbus, ips, 502);
            ModbusTCPSlave slave = new ModbusTCPSlave("slave", "Тестовый слайв", regsModbus, 502);
            Reconnect.StartReconnect(10000);
            slave.Init(500, 1000);
            master.Init(500, 1000);
            slave.Start();
            master.Start();
            for(int i = 0; i < 5; i++)
            {
                float tfloat = float.Parse("123,12345");
                string ff = tfloat.ToString();

                slave.SetValue("A2MD12LP1", "true");
                slave.SetValue("B8IS22LDU", "true");
                slave.SetValue("R1VS01IDU", "100");
                slave.SetValue("A3CP02RDU", "123,1234");
                Thread.Sleep(1000);

                Console.WriteLine(master.GetValue("A2MD12LP1")+"\t"+
                master.GetValue("B8IS22LDU")+"\t"+
                master.GetValue("R1VS01IDU") + "\t" +
                master.GetValue("A3CP02RDU") + "\t" );
            }
            for(int i = 0; i < Reconnect.RowsCount(); i++)
            {
                string[] r = Reconnect.Row(i);
                Console.WriteLine(r[0] + "\t" + r[1] + "\t" + r[2]);
            }
            Reconnect.Stop();
            slave.Stop();
            master.Stop();
            Console.WriteLine(" ok done");
            return true;
        }

        private bool TestXMLModbus()
        {
            Console.Write("Testing class XMLModbus ");
            Dictionary<string, ModbusRegister> regsModbus = XMLModbus.Load(".../../../test/du.xml");
            foreach (ModbusRegister reg in regsModbus.Values)
            {
 //                              Console.WriteLine(reg.Name);
            }
            Console.WriteLine("Testing classes ModbusTCP Master and Slave ok done");
            return true;
        }

        private bool TestXMLApax()
        {
            Console.Write("Testing class XMLApax ");
            Dictionary<string, ApaxRegister> regsApax = XMLApax.Load(".../../../test/ButtonL.xml");
            foreach(ApaxRegister reg in regsApax.Values)
            {
 //               Console.WriteLine(reg.Name);
            }
            Console.WriteLine(" ok done");
            return true;
        }

        private bool TestApaxRegister()
        {
            Console.Write("Testing class ApaxRegister ");
            bool[] b = new bool[32 * 24];
            Variable varbool = new Variable("TESTVARBOOL", "Проверка переменной bool", Util.TYPEVAR.BOOLEAN);
            ApaxRegister regap = new ApaxRegister("TAPAX", "Проверочка APAX", 1, 0, 3);
            varbool.SetVarValue("true false true");
            varbool.SetVarValue("true false true");
            regap.SetAsBool(ref b, varbool.GetVarValue());
            varbool.SetVarValue(regap.GetAsBool(ref b));
            if (!bool.Parse(varbool.GetVarValueComponent(2)))
            {
                Console.WriteLine("Convert bool is bad ");
                return false;
            }
            Console.WriteLine(" ok done");
            return true;
        }
    }
}
