using System;
using System.Collections.Generic;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("RubyModule: " + nameof(Name))]
    public sealed class RubyModule : RubyObject
    {
        public string Name { get; }
        public RubySymbol Symbol { get; }
        private static readonly Dictionary<string, RubyModule> modules = new Dictionary<string, RubyModule>();

        private RubyModule(string module_name)
        {
            Name = module_name;
            Symbol = RubySymbol.GetSymbol(module_name);
            ClassName = RubySymbol.GetSymbol("Module");
            modules.Add(module_name, this);
        }

        public static Dictionary<string, RubyModule> GetModules() { return modules; }

        public static RubyModule GetModule(RubySymbol module_name) { return GetModule(module_name.Name); }

        public static RubyModule GetModule(string module_name) { return modules.ContainsKey(module_name) ? modules[module_name] : new RubyModule(module_name); }

        public override string ToString() { return $"RubyModule: {Name}"; }
    }
}