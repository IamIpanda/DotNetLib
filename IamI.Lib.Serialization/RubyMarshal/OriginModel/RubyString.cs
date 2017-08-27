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
        protected bool setByText;
        protected bool setByRaw;

        public RubyString(string unicodeText)
        {
            encoding = Encoding.Unicode;
            str = unicodeText;
            setByText = true;
            ClassName = RubySymbol.GetSymbol("String");
            Encoding = Encoding.UTF8;
        }

        public RubyString(byte[] raw)
        {
            this.raw = raw;
            encoding = Encoding.Default;
            setByRaw = true;
            ClassName = RubySymbol.GetSymbol("String");
        }

        public RubyString(byte[] raw, Encoding encoding)
        {
            this.raw = raw;
            this.encoding = encoding;
            setByRaw = true;
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
                if (setByRaw) return raw;
                if (encoding == null) throw new NotSupportedException();
                setByText = false;
                setByRaw = true;
                raw = encoding.GetBytes(str);
                return raw;
            }
            set
            {
                raw = value;
                setByText = false;
                setByRaw = true;
            }
        }

        public string Text
        {
            get
            {
                if (setByText) return str;
                if (encoding == Encoding.Default) return Encoding.Default.GetString(raw);
                setByRaw = false;
                setByText = true;
                Text = encoding.GetString(raw);
                return str;
            }
            set
            {
                str = value;
                if (encoding == null) encoding = Encoding.Unicode;
                setByText = true;
                setByRaw = false;
            }
        }

        public string RawText => Encoding.Default.GetString(raw);

        public override Encoding Encoding { get { return encoding; } set { encoding = value; } }

        public override string ToString() { return Text; }

        public override object Clone() { return setByRaw ? new RubyString(raw, encoding) : new RubyString(str); }

        public static explicit operator string(RubyString self) { return self.Text; }
    }
}