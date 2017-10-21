using System;
using System.Collections.Generic;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("RubyMarshal::Module: " + nameof(Name))]
    public sealed class RubyModule : RubyObject
    {
        public string Name { get; }
        public RubySymbol Symbol { get; }
        private static readonly Dictionary<string, RubyModule> Modules = new Dictionary<string, RubyModule>();

        private RubyModule(string module_name)
        {
            Name = module_name;
            Symbol = RubySymbol.GetSymbol(module_name);
            ClassName = RubySymbol.GetSymbol("Module");
            Modules.Add(module_name, this);
        }

        public static Dictionary<string, RubyModule> GetModules() { return Modules; }

        public static RubyModule GetModule(RubySymbol module_name) { return GetModule(module_name.Name); }

        public static RubyModule GetModule(string module_name) { return Modules.ContainsKey(module_name) ? Modules[module_name] : new RubyModule(module_name); }

        public override string ToString() { return $"RubyModule: {Name}"; }
    }
}