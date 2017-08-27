using System;
using System.Globalization;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    public sealed class RubyFloat : RubyObject
    {
        public RubyFloat(double value)
        {
            Value = value;
            ClassName = RubySymbol.GetSymbol("Float");
        }

        public RubyFloat(float value)
        {
            Value = value;
            ClassName = RubySymbol.GetSymbol("Float");
        }

        public RubyFloat() : this(0) { }

        public override string ToString() { return Value.ToString(CultureInfo.InvariantCulture); }

        public double Value { get; }
    }
}