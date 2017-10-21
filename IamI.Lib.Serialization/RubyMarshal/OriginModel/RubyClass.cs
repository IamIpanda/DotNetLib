using System;
using System.Collections.Generic;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("RubyMarshal::Class: " + nameof(Name))]
    public sealed class RubyClass : RubyObject
    {
        public string Name { get; }
        public RubySymbol Symbol { get; }
        private static readonly Dictionary<string, RubyClass> Classes = new Dictionary<string, RubyClass>();

        private RubyClass(string class_name)
        {
            Name = class_name;
            Symbol = RubySymbol.GetSymbol(class_name);
            ClassName = RubySymbol.GetSymbol("Class");
            Classes.Add(class_name, this);
        }

        public static Dictionary<string, RubyClass> GetClasses() { return Classes; }

        public static RubyClass GetClass(RubySymbol class_name) { return GetClass(class_name.Name); }

        public static RubyClass GetClass(string class_name) { return Classes.ContainsKey(class_name) ? Classes[class_name] : new RubyClass(class_name); }

        public override string ToString() { return Name; }
    }
}