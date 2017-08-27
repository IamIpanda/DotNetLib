using System;
using System.Collections.Generic;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("RubyClass: " + nameof(Name))]
    public sealed class RubyClass : RubyObject
    {
        public string Name { get; }
        public RubySymbol Symbol { get; }
        private static readonly Dictionary<string, RubyClass> classes = new Dictionary<string, RubyClass>();

        private RubyClass(string class_name)
        {
            Name = class_name;
            Symbol = RubySymbol.GetSymbol(class_name);
            ClassName = RubySymbol.GetSymbol("Class");
            classes.Add(class_name, this);
        }

        public static Dictionary<string, RubyClass> GetClasses() { return classes; }

        public static RubyClass GetClass(RubySymbol class_name) { return GetClass(class_name.Name); }

        public static RubyClass GetClass(string class_name) { return classes.ContainsKey(class_name) ? classes[class_name] : new RubyClass(class_name); }

        public override string ToString() { return Name; }
    }
}