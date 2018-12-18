using System.Collections.Generic;
using loggers;
using Function;
namespace inout
{
    public class ServerOPU
    {
        public string name;
        public string descriptions;
        public int stepCycle;
        public int stepReconnect;
        private Dictionary<string, Driver> drvs;
        private List<Blind> blinds;
        private Dictionary<string, Variable> vars;
        private bool LoadingError;
        private const string ClassName = "ServerOPU";

        public ServerOPU(string name, string descriptions, int stepCycle, int stepReconnect)
        {
            this.name = name;
            this.descriptions = descriptions;
            this.stepCycle = stepCycle;
            this.stepReconnect = stepReconnect;
            drvs = new Dictionary<string, Driver>();
            blinds = new List<Blind>();
            vars = new Dictionary<string, Variable>();
            LoadingError = false;
        }
        public void StartAllDevices()
        {
            foreach (Driver drv in drvs.Values)
            {
                drv.Start();
            }
        }
        public void StopAllDevices()
        {
            foreach (Driver drv in drvs.Values)
            {
                drv.Stop();
            }
        }
        public void LoadVariablesFromDevices()
        {
            foreach (Variable var in vars.Values)
            {
                string[] name = var.GetName().Split(':');
                if (name.Length == 1) continue;
                if (!var.LoadFromDevice) return;
                Driver drv;
                if(!drvs.TryGetValue(name[0],out drv))
                {
                    Log.Fatal(ClassName, "Нет такого устройства " + name[0]);
                    continue;
                }
                var.SetVarValue(drv.GetValue(name[1]));
            }
        }
        public void SaveVariablesToDevices()
        {
            foreach (Variable var in vars.Values)
            {
                //if (!var.IsChanged()) continue;
                string[] name = var.GetName().Split(':');
                if (name.Length == 1) continue;
                if (!var.SaveToDevice) return;
                Driver drv;
                if (!drvs.TryGetValue(name[0], out drv))
                {
                    Log.Fatal(ClassName, "Нет такого устройства " + name[0]);
                    continue;
                }
                drv.SetValue(name[1], var.GetVarValue());
                var.NewCycle();
            }

        }
        public void MakeOneStep()
        {
            Variable var;
            foreach (Blind blnd in blinds)
            {
                List<string> pars = new List<string>();
                foreach (string str in blnd.paramNames)
                {
                    if(!vars.TryGetValue(str,out var))
                    {
                        Log.Fatal(ClassName, "Не найдена переменная " + str);
                        continue;
                    }
                    pars.Add(var.GetVarValue());
                }
                string result = Funct.DoIt(blnd.function, pars);
                if (!vars.TryGetValue(blnd.resultName, out var))
                {
                    Log.Fatal(ClassName, "Не найдена переменная " + blnd.resultName);
                    continue;
                }
                var.SetVarValue(result);
                vars[blnd.resultName] = var;
            }
        }
        public bool isError() => LoadingError;
        public void AddDriver(Driver drv, int step, int timeout)
        {
            drvs.Add(drv.GetName(), drv);
            drv.Init(step, timeout);
        }
        public void AddBlind(Blind blnd)
        {
            if (!Function.Funct.isPresent(blnd.function))
            {
                Log.Fatal("Загрузка", "в " + blnd.ToString() + " нет такой функции "+blnd.function);
                LoadingError = true;
            }
            string newname;
            if (!VerifyNames(blnd.resultName, out newname,true))
            {
                Log.Fatal("Загрузка", "в " + blnd.ToString() + " неверно задано имя " + blnd.resultName);
                LoadingError = true;
            }
            blnd.resultName = newname;
            for (int i = 0; i < blnd.paramNames.Count; i++)
            {
                if (!VerifyNames(blnd.paramNames[i], out newname,false))
                {
                    Log.Fatal("Загрузка", "в " + blnd.ToString() + " неверно задано имя " + blnd.paramNames[i]);
                    LoadingError = true;
                    continue;
                }
                blnd.paramNames[i] = newname;
            }
            blinds.Add(blnd);
        }

        public bool VerifyNames(string anyname, out string newname,bool output)
        {
            string[] nn = anyname.Split(':');
            newname = anyname;
            if (nn.Length == 2)
            {
                if (!drvs.ContainsKey(nn[0]))
                {
                    return false;
                }

                Driver drv = drvs[nn[0]];
                if (!drv.IsHaveVariable(nn[1])) return false;
                string desc = drv.GetDescription(nn[1]);
                if (!vars.ContainsKey(newname))
                {
                    Variable var = new Variable(newname, desc, drv.GetTypeVar(nn[1]));
                    if (output) var.SaveToDevice = true; ;
                    if (!output) var.LoadFromDevice = true;
                    vars.Add(newname, var);
                }
                return true;
            }
            if (nn.Length == 1)
            {
                if (vars.ContainsKey(nn[0])) return true;
                foreach(Driver drv in drvs.Values)
                {
                    if (drv.IsHaveVariable(nn[0]))
                    {
                        string desc = drv.GetDescription(nn[0]);
                        newname = drv.GetName() + ":" + nn[0];
                        if (!vars.ContainsKey(newname))
                        {
                            Variable var = new Variable(newname, desc, drv.GetTypeVar(nn[0]));
                            if (output) var.SaveToDevice = true; ;
                            if (!output) var.LoadFromDevice = true;
                            vars.Add(newname, var);
                        }
                        return true;
                    }
                }
                return false; 
            }
            return false;
        }
        public void AddVariable(Variable var)
        {
            vars.Add(var.GetName(), var);
        }
        public List<string> ListAllBlaind()
        {
            List<string> result = new List<string>();
            foreach(Blind blnd in blinds)
            {
                result.Add(blnd.ToString());
            }
            return result;
        }
        public List<Variable> ListAllVariables()
        {
            List<Variable> result = new List<Variable>(vars.Values.Count);
            foreach (Variable var in vars.Values)
            {
                result.Add(var);
            }
            return result;
        }
        public Dictionary<string, Driver> ListAllDevices()
        {
            return drvs;
        }
    }
}
