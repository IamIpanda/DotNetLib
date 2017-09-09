using System;
using System.Collections.Generic;
using System.Text;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [System.Diagnostics.DebuggerDisplay("RubyMarshal::String {" + nameof(Text) + "}")]
    [System.Diagnostics.DebuggerTypeProxy(typeof(RubyStringDebugView))]
    [Serializable]
    public class RubyString : RubyObject
    {
        [Serializable]
        internal class RubyStringDebugView
        {
            internal RubyString str;

            public RubyStringDebugView(RubyString str) { this.str = str; }

            public string Text => str.Text;

            public Encoding Encoding => str.Encoding;

            public RubySymbol ClassName => str.ClassName;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<RubySymbol, object>[] Keys
            {
                get
                {
                    var keys = new KeyValuePair<RubySymbol, object>[str.InstanceVariables.Count];

                    var i = 0;
                    foreach (var key in str.InstanceVariables)
                    {
                        keys[i] = key;
                        i++;
                    }
                    return keys;
                }
            }
        }

        protected byte[] raw;
        protected Encoding encoding;
        protected string str;
        protected bool set_by_text;
        protected bool set_by_raw;

        public RubyString(string unicode_text)
        {
            encoding = Encoding.Unicode;
            str = unicode_text;
            set_by_text = true;
            ClassName = RubySymbol.GetSymbol("String");
            Encoding = Encoding.UTF8;
        }

        public RubyString(byte[] raw)
        {
            this.raw = raw;
            encoding = Encoding.Default;
            set_by_raw = true;
            ClassName = RubySymbol.GetSymbol("String");
        }

        public RubyString(byte[] raw, Encoding encoding)
        {
            this.raw = raw;
            this.encoding = encoding;
            set_by_raw = true;
            ClassName = RubySymbol.GetSymbol("String");
        }

        public RubyString() : this("") { }

        public RubyString ForceEncoding(Encoding target_encoding)
        {
            Encoding = target_encoding;
            return this;
        }

        public RubyString Encode(Encoding target_encoding)
        {
            Text = Text;
            Encoding = target_encoding;
            return this;
        }

        public byte[] Raw
        {
            get
            {
                if (set_by_raw) return raw;
                set_by_text = false;
                set_by_raw = true;
                raw = encoding?.GetBytes(str) ?? throw new NotSupportedException();
                return raw;
            }
            set
            {
                raw = value;
                set_by_text = false;
                set_by_raw = true;
            }
        }

        public string Text
        {
            get
            {
                if (set_by_text) return str;
                if (Equals(encoding, Encoding.Default)) return Encoding.Default.GetString(raw);
                set_by_raw = false;
                set_by_text = true;
                Text = encoding.GetString(raw);
                return str;
            }
            set
            {
                str = value;
                if (encoding == null) encoding = Encoding.Unicode;
                set_by_text = true;
                set_by_raw = false;
            }
        }

        public string RawText => Encoding.Default.GetString(raw);

        public override Encoding Encoding { get { return encoding; } set { encoding = value; } }

        public override string ToString() { return Text; }

        public override object Clone() { return set_by_raw ? new RubyString(raw, encoding) : new RubyString(str); }

        public static explicit operator string(RubyString self) { return self.Text; }
    }
}