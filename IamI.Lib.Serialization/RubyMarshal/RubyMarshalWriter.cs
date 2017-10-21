using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IamI.Lib.Serialization.RubyMarshal.OriginModel;

namespace IamI.Lib.Serialization.RubyMarshal
{
    public class RubyMarshalWriter
    {
        private readonly Dictionary<object, object> _m_compat_tbl = new Dictionary<object, object>();
        private readonly Dictionary<object, int> _m_objects = new Dictionary<object, int>();
        private readonly Stream _m_stream;
        private readonly Dictionary<RubySymbol, int> _m_symbols = new Dictionary<RubySymbol, int>();
        private readonly BinaryWriter _m_writer;

        public RubyMarshalWriter(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (!output.CanWrite) throw new ArgumentException("stream cannot write");
            _m_stream = output;
            _m_writer = new BinaryWriter(_m_stream);
        }

        public RubyMarshal.StringStyleType StringStyle { get; set; } = RubyMarshal.StringStyleType.Style19;

        /// <summary>
        ///     static void w_nbyte(const char *s, long n, struct dump_arg *arg)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        public void WriteNByte(byte[] s, int n)
        {
            _m_writer.Write(s, 0, n);
        }

        /// <summary>
        ///     static void w_byte(char c, struct dump_arg *arg)
        /// </summary>
        /// <param name="c"></param>
        public void WriteByte(byte c)
        {
            _m_writer.Write(c);
        }

        public static void WriteLong(long value, BinaryWriter writer)
        {
            if (value == 0)
            {
                writer.Write((byte) 0);
            }
            else if (value > 0 && value < 0x7b)
            {
                writer.Write((byte) (value + 5));
            }
            else if (value < 0 && value > -124)
            {
                writer.Write((sbyte) (value - 5));
            }
            else
            {
                sbyte num2;
                var buffer = new byte[5];
                buffer[1] = (byte) (value & 0xff);
                buffer[2] = (byte) ((value >> 8) & 0xff);
                buffer[3] = (byte) ((value >> 0x10) & 0xff);
                buffer[4] = (byte) ((value >> 0x18) & 0xff);
                var index = 4;
                if (value >= 0)
                {
                    while (buffer[index] == 0) index--;
                    num2 = (sbyte) index;
                }
                else
                {
                    while (buffer[index] == 0xff) index--;
                    num2 = (sbyte) -index;
                }
                buffer[0] = (byte) num2;
                writer.Write(buffer, 0, index + 1);
            }
        }

        /// <summary>
        ///     static void w_long(long x, struct dump_arg *arg)
        /// </summary>
        /// <param name="value"></param>
        public void WriteLong(long value)
        {
            WriteLong(value, _m_writer);
        }

        /// <summary>
        ///     static void w_bytes(const char *s, long n, struct dump_arg *arg)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        public void WriteBytes(byte[] s, int n)
        {
            WriteLong(n);
            WriteNByte(s, n);
        }

        public void WriteBytes(byte[] s)
        {
            WriteBytes(s, s.Length);
        }

        /// <summary>
        ///     #define w_cstr(s, arg) w_bytes((s), strlen(s), (arg))
        /// </summary>
        /// <param name="s"></param>
        public void WriteCString(string s)
        {
            WriteLong(s.Length);
            _m_writer.Write(Encoding.Default.GetBytes(s));
        }

        /// <summary>
        ///     static void w_float(double d, struct dump_arg *arg)
        /// </summary>
        /// <param name="value"></param>
        public void WriteFloat(double value)
        {
            if (double.IsInfinity(value)) WriteCString(double.IsPositiveInfinity(value) ? "inf" : "-inf");
            else if (double.IsNaN(value)) WriteCString("nan");
            else WriteCString($"{value:g}");
        }

        public void WriteFloat(float value)
        {
            WriteFloat((double) value);
        }

        public void WriteFloat(RubyFloat value)
        {
            WriteFloat(value.Value);
        }

        /// <summary>
        ///     static void w_symbol(ID id, struct dump_arg *arg)
        /// </summary>
        /// <param name="id"></param>
        public void WriteSymbol(RubySymbol id)
        {
            int num;

            if (_m_symbols.TryGetValue(id, out num))
            {
                WriteByte(RubyMarshal.Types.SYMBOL_LINK);
                WriteLong(num);
            }
            else
            {
                var sym = id.Name;
                if (sym.Length == 0) throw new InvalidDataException("can't dump anonymous ID");
                var encidx = id.Encoding;
                if (encidx == Encoding.ASCII || encidx == Encoding.Default || encidx == Encoding.UTF8) encidx = null;
                if (encidx != null) WriteByte(RubyMarshal.Types.INSTANCE_VARIABLE);
                WriteByte(RubyMarshal.Types.SYMBOL);
                WriteCString(sym);

                _m_symbols.Add(id, _m_symbols.Count);
                if (encidx != null) WriteEncoding(id, 0);
            }
        }

        /// <summary>
        ///     static void w_unique(VALUE s, struct dump_arg *arg)
        /// </summary>
        /// <param name="s"></param>
        public void WriteUnique(RubySymbol s)
        {
            WriteSymbol(s);
        }

        /// <summary>
        ///     static void w_extended(VALUE klass, struct dump_arg *arg, int check)
        /// </summary>
        /// <param name="klass"></param>
        /// <param name="check"></param>
        public void WriteExtended(object klass, bool check)
        {
            if (!(klass is RubyObject fobj)) return;
            foreach (var item in fobj.ExtendModules)
            {
                WriteByte(RubyMarshal.Types.EXTENDED);
                WriteUnique(item.Symbol);
            }
        }

        /// <summary>
        ///     static void w_class(char type, VALUE obj, struct dump_arg *arg, int check)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="check"></param>
        public void WriteClass(byte type, object obj, bool check)
        {
            if (_m_compat_tbl.TryGetValue(obj, out var real_obj)) obj = real_obj;
            if (!(obj is RubyObject fobj)) return;
            var klass = RubyClass.GetClass(fobj.ClassName);
            WriteExtended(klass, check);
            WriteByte(type);
            WriteUnique(fobj.ClassName);
        }

        /// <summary>
        ///     static void w_uclass(VALUE obj, VALUE super, struct dump_arg *arg)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="super"></param>
        public void WriteUserClass(object obj, RubyClass super)
        {
            if (obj is RubyObject fobj)
            {
                var klass = fobj.Class;
                WriteExtended(klass, true);
                if (klass == super) return;
                WriteByte(RubyMarshal.Types.USER_CLASS);
                WriteUnique(klass.Symbol);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        ///     static void w_encoding(VALUE obj, long num, struct dump_call_arg *arg)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="num"></param>
        public void WriteEncoding(object obj, int num)
        {
            Encoding encidx = null;
            if (obj is RubyObject ruby_object) encidx = ruby_object.Encoding;
            if (encidx == null)
            {
                WriteLong(num);
                return;
            }
            WriteLong(num + 1);

            if (Equals(encidx, Encoding.Default))
            {
                /* special treatment for US-ASCII and UTF-8 */
                WriteSymbol(RubyMarshal.Ds.e);
                WriteObject(false);
                return;
            }
            if (Equals(encidx, Encoding.UTF8))
            {
                WriteSymbol(RubyMarshal.Ds.e);
                WriteObject(true);
                return;
            }

            WriteSymbol(RubyMarshal.Ds.encoding);
            WriteObject(RubySymbol.GetSymbol(encidx.BodyName));
        }

        /// <summary>
        ///     static void w_ivar(VALUE obj, st_table *tbl, struct dump_call_arg *arg)
        /// </summary>
        /// <param name="obj"></param>
        public void WriteInstanceVariable(RubyObject obj, Dictionary<RubySymbol, object> tbl)
        {
            var num = tbl?.Count ?? 0;
            if (tbl != null && tbl.ContainsKey(RubyMarshal.Ds.encoding) && num > 0) num -= 1;
            if (tbl != null && tbl.ContainsKey(RubyMarshal.Ds.e) && num > 0) num -= 1;

            WriteEncoding(obj, num);
            if (tbl == null) return;
            foreach (var item in tbl)
            {
                if (item.Key == RubyMarshal.Ds.encoding) continue;
                if (item.Key == RubyMarshal.Ds.e) continue;
                WriteSymbol(item.Key);
                WriteObject(item.Value);
            }
        }

        /// <summary>
        ///     static void w_objivar(VALUE obj, struct dump_call_arg *arg)
        /// </summary>
        /// <param name="obj"></param>
        public void WriteObjectInstanceVariable(RubyObject obj)
        {
            WriteInstanceVariable(obj, obj.InstanceVariables);
        }

        /// <summary>
        ///     static void w_object(VALUE obj, struct dump_arg *arg, int limit)
        /// </summary>
        /// <param name="obj"></param>
        public void WriteObject(object obj)
        {
            int num;
            if (_m_objects.TryGetValue(obj, out num))
            {
                WriteByte(RubyMarshal.Types.LINK);
                WriteLong(num);
                return;
            }
            if (obj == null || obj == RubyNil.Instance)
            {
                WriteByte(RubyMarshal.Types.NIL);
            }
            else if (obj is bool && (bool) obj)
            {
                WriteByte(RubyMarshal.Types.TRUE);
            }
            else if (obj is bool && (bool) obj == false)
            {
                WriteByte(RubyMarshal.Types.FALSE);
            }
            else if (obj is RubyBool ruby_bool)
            {
                WriteByte(ruby_bool.Value ? RubyMarshal.Types.TRUE : RubyMarshal.Types.FALSE);
            }
            else if (obj is int || obj is long || obj is RubyFixnum)
            {
                long v;
                if (obj is int | obj is long) v = (long) obj;
                else v = ((RubyFixnum) obj).Value;
                // (2**30).class   => Bignum
                // (2**30-1).class => Fixnum
                // (-2**30-1).class=> Bignum
                // (-2**30).class  => Fixnum
                if (v <= RubyFixnum.MaxValue && v >= RubyFixnum.MinValue)
                {
                    WriteByte(RubyMarshal.Types.FIXNUM);
                    WriteLong((int) v);
                }
                else
                {
                    WriteObject(RubyBignum.Create(v));
                }
            }
            else if (obj is RubySymbol ruby_symbol)
            {
                WriteSymbol(ruby_symbol);
            }

            else
            {
                var fobj = obj as RubyObject;
                var hasiv = false;
                if (fobj != null) hasiv = (obj is RubyArray || obj is RubyHash) && fobj.InstanceVariables.Count > 0;
                if (fobj is RubyString ruby_string)
                    hasiv |= StringStyle == RubyMarshal.StringStyleType.Style19 && fobj.Encoding != null &&
                             ruby_string.Text.Length > 0;

                if (obj is DefaultRubyUserDefinedMarshalDumpObject default_ruby_user_defined_marshal_dump_object)
                {
                    if (hasiv) WriteByte(RubyMarshal.Types.INSTANCE_VARIABLE);
                    WriteClass(RubyMarshal.Types.USER_MARSHAL, obj, false);
                    default_ruby_user_defined_marshal_dump_object.Write(_m_writer);
                    if (hasiv) WriteObjectInstanceVariable(fobj);
                    _m_objects.Add(obj, _m_objects.Count);
                    return;
                }
                if (obj is RubyUserDefinedObject ruby_user_defined_object)
                {
                    if (hasiv) WriteByte(RubyMarshal.Types.INSTANCE_VARIABLE);
                    _m_writer.Write(RubyMarshal.Types.USER_DEFINED);
                    WriteSymbol(fobj.ClassName);
                    ruby_user_defined_object.Write(_m_writer);
                    if (hasiv) WriteObjectInstanceVariable(fobj);
                    _m_objects.Add(obj, _m_objects.Count);
                    return;
                }

                _m_objects.Add(obj, _m_objects.Count);

                if (hasiv) WriteByte(RubyMarshal.Types.INSTANCE_VARIABLE);

                if (obj is RubyClass ruby_class)
                {
                    WriteByte(RubyMarshal.Types.CLASS);
                    WriteCString(ruby_class.Name);
                }
                else if (obj is RubyModule ruby_module)
                {
                    WriteByte(RubyMarshal.Types.MODULE);
                    WriteCString(ruby_module.Name);
                }
                else if (obj is float _float)
                {
                    WriteByte(RubyMarshal.Types.FLOAT);
                    WriteFloat(_float);
                }
                else if (obj is double _double)
                {
                    WriteByte(RubyMarshal.Types.FLOAT);
                    WriteFloat(_double);
                }
                else if (obj is RubyFloat ruby_float)
                {
                    WriteByte(RubyMarshal.Types.FLOAT);
                    WriteFloat(ruby_float);
                }
                else if (obj is RubyBignum ruby_bignum)
                {
                    char ch;
                    if (ruby_bignum.Sign > 0) ch = '+';
                    else if (ruby_bignum.Sign < 0) ch = '-';
                    else ch = '0';
                    _m_writer.Write((byte) ch);
                    var words = ruby_bignum.GetWords();
                    var num2 = words.Length * 2;
                    var index = words.Length - 1;
                    /*
                    var flag = false;
                    if (words.Length > 0 && words[index] >> 0x10 == 0)
                    {
                        num--;
                        flag = true;
                    }*/
                    var flag = words.Length > 0 && words[index] >> 0x10 == 0;
                    WriteLong(num2);
                    for (var i = 0; i < words.Length; i++)
                        if (flag && i == index) _m_writer.Write((ushort) words[i]);
                        else _m_writer.Write(words[i]);
                }
                else if (obj is RubyString || obj is string)
                {
                    RubyString v;
                    if (obj is string _string) v = new RubyString(_string);
                    else v = (RubyString) obj;
                    WriteUserClass(v, RubyClass.GetClass("String"));
                    WriteByte(RubyMarshal.Types.STRING);
                    WriteBytes(v.Raw);
                }
                else if (obj is RubyRegexp ruby_regexp)
                {
                    WriteUserClass(obj, RubyClass.GetClass("Regexp"));
                    WriteByte(RubyMarshal.Types.REGEXP);
                    WriteBytes(ruby_regexp.pattern.Raw);
                    WriteByte((byte) ruby_regexp.options);
                }
                else if (obj is RubyArray || obj is List<object>)
                {
                    RubyArray v;
                    if (obj is List<object> list) v = new RubyArray(list);
                    else v = (RubyArray) obj;
                    WriteUserClass(v, RubyClass.GetClass("Array"));
                    WriteByte(RubyMarshal.Types.ARRAY);
                    WriteLong(v.Length);
                    foreach (var t in v) WriteObject(t);
                }
                else if (obj is RubyHash ruby_hash)
                {
                    WriteUserClass(obj, RubyClass.GetClass("Hash"));
                    WriteByte(ruby_hash.DefaultValue != null
                        ? RubyMarshal.Types.HASH_WITH_DEFAULT
                        : RubyMarshal.Types.HASH);
                    WriteLong(ruby_hash.Length);
                    foreach (var item in ruby_hash)
                    {
                        WriteObject(item.Key);
                        WriteObject(item.Value);
                    }
                    if (ruby_hash.DefaultValue != null) WriteObject(ruby_hash.DefaultValue);
                }
                else if (obj is RubyStruct ruby_struct)
                {
                    WriteUserClass(obj, RubyClass.GetClass("Struct"));
                    WriteLong(ruby_struct.InstanceVariables.Count);
                    foreach (var item in ruby_struct.InstanceVariables)
                    {
                        WriteObject(item.Key);
                        WriteObject(item.Value);
                    }
                }
                else if (obj is RubyObject ruby_object)
                {
                    WriteClass(RubyMarshal.Types.OBJECT, obj, true);
                    WriteObjectInstanceVariable(ruby_object);
                }
                else
                {
                    throw new InvalidDataException($"can't dump {obj.GetType().FullName}");
                }
                if (hasiv) WriteInstanceVariable(fobj, fobj.InstanceVariables);
            }
        }

        public void Dump(object obj)
        {
            _m_writer.Write(RubyMarshal.MARSHAL_MAJOR);
            _m_writer.Write(RubyMarshal.MARSHAL_MINOR);
            WriteObject(obj);
            _m_stream.Flush();
        }
    }
}