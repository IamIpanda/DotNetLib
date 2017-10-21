using System;
using System.Collections.Generic;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("RubyMarshal::Symbol {" + nameof(Name) + "}")]
    public class RubySymbol : RubyObject
    {
        private static readonly Dictionary<string, RubySymbol> Symbols = new Dictionary<string, RubySymbol>();
        internal static RubySymbol symbol_class_name = new RubySymbol();
        public string Name { get; }
        internal RubyString ruby_string;

        protected RubySymbol(string symbol_name)
        {
            Name = symbol_name;
            Symbols.Add(symbol_name, this);
        }

        private RubySymbol()
        {
            Name = "Symbol";
            Symbols.Add(Name, this);
        }

        public override RubySymbol ClassName => symbol_class_name;

        public static Dictionary<string, RubySymbol> GetSymbols() { return Symbols; }

        /// <summary>
        /// 以给定的 symbol_name 为名，查找该 RubySymbol。
        /// 若不存在，则会创建新的 RubySymbol。
        /// </summary>
        /// <param name="symbol_name">要查找的 RubySymbol 的名称。</param>
        /// <returns>Name = symbol_name 的 RubySymbol 对象。</returns>
        public static RubySymbol GetSymbol(string symbol_name) { return Symbols.ContainsKey(symbol_name) ? Symbols[symbol_name] : new RubySymbol(symbol_name); }

        public static RubySymbol GetSymbol(RubyString str)
        {
            var s = str.Text;
            if (Symbols.ContainsKey(s)) return Symbols[s];
            var sym = new RubySymbol(s) {ruby_string = str};
            return sym;
        }

        public RubyString RubyString() => ruby_string;

        public override string ToString() { return ":" + Name; }

        public override RubyClass Class => RubyClass.GetClass(this);

        /// <summary>
        /// 请注意 RubySymbol 不可被复制。
        /// 此函数将返回自身。
        /// </summary>
        /// <returns>调用方本身</returns>
        public override object Clone() { return this; }
    }
}