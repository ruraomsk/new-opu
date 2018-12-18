using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace inout
{
    public class Driver
    {
        public string name="unknow";
        public string description ="driver not named";
        public bool Connect = true;
        public string typeDriver = "unknow";
        public virtual void Init(int step,int timeout) { }
        public virtual void Start() { }
        public virtual void Stop() { }
        public virtual void Run() { }
        public virtual bool SetValue(string nameValue, string value) { return false; }
        public virtual string GetValue(string nameValue) => null;

        public virtual Util.TYPEVAR GetTypeVar(string nameValue) => Util.TYPEVAR.BOOLEAN;
        public virtual string GetDescription(string nameValue) => "";

        public bool IsConnected() => Connect;
        public string GetName() => name;
        public string GetDescription() => description;

        public virtual bool IsHaveVariable(string nameValue) => false;
        public virtual void Reconect()
        {
            Stop();
            Start();
        }
        public virtual string Status() => null;
        public virtual string[] Row(int row) => null;
        public virtual int RowsCount() => 0;
        public virtual string[] ColumnsName() => null;
    }
}
