using System;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("RubyMarshal::Struct")]
    public class RubyStruct : RubyObject
    {
        public RubyStruct() { ClassName = RubySymbol.GetSymbol("Struct"); }
    }
}