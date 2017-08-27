using System;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    public class RubyStruct : RubyObject
    {
        public RubyStruct() { ClassName = RubySymbol.GetSymbol("Struct"); }
    }
}