using System;
using System.Text;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [System.Diagnostics.DebuggerDisplay("RubyMarshal::Regexp /{" + nameof(pattern) + "}/")]
    [Serializable]
    public class RubyRegexp : RubyObject
    {
        public RubyRegexpOptions options;
        public RubyString pattern;

        public RubyRegexp(RubyString pattern, RubyRegexpOptions options)
        {
            this.pattern = pattern;
            this.options = options;
            ClassName = RubySymbol.GetSymbol("Regexp");
        }

        public RubyRegexp() : this(new RubyString(), new RubyRegexpOptions()) { }

        public override Encoding Encoding { get { return pattern.Encoding; } set { pattern.Encoding = value; } }
        public override object Clone() { return new RubyRegexp(pattern.Clone() as RubyString, options); }

        public override string ToString() { return $"/{pattern}/"; }
    }

    [Flags]
    public enum RubyRegexpOptions
    {
        None = 0, // #define ONIG_OPTION_NONE               0U
        IgnoreCase = 1, // #define ONIG_OPTION_IGNORECASE         1U
        Extend = 2, // #define ONIG_OPTION_EXTEND             (ONIG_OPTION_IGNORECASE         << 1)
        Multiline = 4, // #define ONIG_OPTION_MULTILINE          (ONIG_OPTION_EXTEND             << 1)
        Singleline = 8, // #define ONIG_OPTION_SINGLELINE         (ONIG_OPTION_MULTILINE          << 1)
        FindLongest = 16, // #define ONIG_OPTION_FIND_LONGEST       (ONIG_OPTION_SINGLELINE         << 1)
        FindNotEmpty = 32, // #define ONIG_OPTION_FIND_NOT_EMPTY     (ONIG_OPTION_FIND_LONGEST       << 1)
    }
}