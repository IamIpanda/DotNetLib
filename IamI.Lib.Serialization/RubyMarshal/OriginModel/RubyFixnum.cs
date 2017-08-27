using System;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    public sealed class RubyFixnum : RubyObject
    {
        public RubyFixnum(long value)
        {
            Value = value;
            ClassName = RubySymbol.GetSymbol("Fixnum");
        }

        public RubyFixnum(int value)
        {
            Value = value;
            ClassName = RubySymbol.GetSymbol("Fixnum");
        }

        public RubyFixnum() : this(0) { }

        public override string ToString() { return Value.ToString(); }

        public long Value { get; }

        public static long MaxValue => 1073741823;
        public static long MinValue => -1073741824;
        public static implicit operator int(RubyFixnum self) { return Convert.ToInt32(self.Value); }
        public static implicit operator long(RubyFixnum self) { return self.Value; }
    }
}