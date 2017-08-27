using System;
using System.Collections.Generic;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("RubyMarshal::Symbol {" + nameof(Name) + "}")]
    public class RubySymbol : RubyObject
    {
        private static readonly Dictionary<string, RubySymbol> symbols = new Dictionary<string, RubySymbol>();
        internal static RubySymbol SymbolClassName = new RubySymbol();
        public string Name { get; }
        internal RubyString ruby_string;

        protected RubySymbol(string symbol_name)
        {
            Name = symbol_name;
            symbols.Add(symbol_name, this);
        }

        private RubySymbol()
        {
            Name = "Symbol";
            symbols.Add(Name, this);
        }

        public override RubySymbol ClassName => SymbolClassName;

        public static Dictionary<string, RubySymbol> GetSymbols() { return symbols; }

        /// <summary>
        /// 以给定的 symbol_name 为名，查找该 RubySymbol。
        /// 若不存在，则会创建新的 RubySymbol。
        /// </summary>
        /// <param name="symbol_name">要查找的 RubySymbol 的名称。</param>
        /// <returns>Name = symbol_name 的 RubySymbol 对象。</returns>
        public static RubySymbol GetSymbol(string symbol_name) { return symbols.ContainsKey(symbol_name) ? symbols[symbol_name] : new RubySymbol(symbol_name); }

        public static RubySymbol GetSymbol(RubyString str)
        {
            var s = str.Text;
            if (symbols.ContainsKey(s)) return symbols[s];
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