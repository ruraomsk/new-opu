using System;
using System.Collections.Generic;

namespace AgentAPI
{
    internal class OPUVariables
    {

        List<Variable> variables= new List<Variable>();

        public OPUVariables(VariablesJson varj)
        {
            variables = varj.variables;
        }

        internal List<string[]> GetAll()
        {
            List<string[]> result = new List<string[]>(variables.Count);
            foreach (Variable v in variables)
            {
                result.Add(v.GetAll());
            }
            return result;
        }

        internal string[] GetColumns()
        {
            return new string[8] { "Name", "Description", "Type", "Position", "Cnanched", "From", "To", "Values" };
;
        }
    }
    internal class VariablesJson
    {
        public string Operation;
        public string Status;
        public List<Variable> variables;
    }
    internal class Variable
    {
        public string Name;
        public string Description;
        public string Type;
        public string Position;
        public string Cnanched;
        public string From;
        public string To;
        public string Values;

        public string[] GetAll()
        {
            return new string[8] { Name, Description, Type, Position, Cnanched, From, To, Values };
        }

    }

}