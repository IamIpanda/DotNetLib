using System;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    public sealed class RubyNil : RubyObject
    {
        private RubyNil() { ClassName = RubySymbol.GetSymbol("NilClass"); }

        public override string ToString() { return "RubyMarshal::Nil"; }

        static RubyNil() { Instance = new RubyNil(); }
        public static RubyNil Instance { get; }
    }
}