using System;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    public sealed class RubyBool : RubyObject
    {
        public static RubyBool True { get; } = new RubyBool(true);
        public static RubyBool False { get; } = new RubyBool(false);
        public bool Value { get; }
        private RubyBool() : this(false) { }

        private RubyBool(bool value)
        {
            Value = value;
            ClassName = RubySymbol.GetSymbol("Bool");
        }

        public override string ToString() { return Value ? "Ruby::True" : "Ruby::False"; }

        public static explicit operator bool(RubyBool self) { return self.Value; }
    }
}