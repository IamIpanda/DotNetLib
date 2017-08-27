using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IamI.Lib.Serialization.RubyMarshal.OriginModel;

namespace IamI.Lib.Serialization.RubyMarshal
{
    public class RubyMarshalReader
    {
        private readonly Stream m_stream;
        private readonly BinaryReader m_reader;
        private readonly Dictionary<int, object> m_objects = new Dictionary<int, object>();
        private readonly Dictionary<int, RubySymbol> m_symbols = new Dictionary<int, RubySymbol>();
        private readonly Dictionary<object, object> m_compat_tbl = new Dictionary<object, object>();
        private readonly Converter<object, object> m_proc = null;

        public RubyMarshalReader(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new ArgumentException("instance of IO needed");
            m_stream = input;
            m_reader = new BinaryReader(m_stream, Encoding.ASCII);
        }

        public object Load()
        {
            var major = ReadByte();
            var minor = ReadByte();
            if (major != RubyMarshal.MarshalMajor || minor > RubyMarshal.MarshalMinor) throw new InvalidDataException($"incompatible marshal file format (can't be read)\n\tformat version {RubyMarshal.MarshalMajor}.{RubyMarshal.MarshalMinor} required; {major}.{minor} given");

            return ReadObject();
        }

        /// <summary>
        /// static int r_byte(struct load_arg *arg)
        /// </summary>
        /// <returns></returns>
        public int ReadByte() { return m_stream.ReadByte(); }

        public static int ReadLong(BinaryReader reader)
        {
            var num = reader.ReadSByte();
            if (num <= -5) return num + 5;
            if (num < 0)
            {
                var output = 0;
                for (var i = 0; i < -num; i++) output += (0xff - reader.ReadByte()) << (8 * i);
                return (-output - 1);
            }
            if (num == 0) return 0;
            if (num <= 4)
            {
                var output = 0;
                for (var i = 0; i < num; i++) output += reader.ReadByte() << (8 * i);
                return output;
            }
            return num - 5;
        }

        /// <summary>
        /// static long r_long(struct load_arg *arg)
        /// </summary>
        /// <returns></returns>
        public int ReadLong()
        {
            return ReadLong(m_reader);
        }

        /// <summary>
        /// static VALUE r_bytes0(long len, struct load_arg *arg)
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public byte[] ReadBytes0(int len) { return m_reader.ReadBytes(len); }

        /// <summary>
        /// #define r_bytes(arg) r_bytes0(r_long(arg), (arg))
        /// </summary>
        /// <returns></returns>
        public byte[] ReadBytes() { return ReadBytes0(ReadLong()); }

        /// <summary>
        /// static ID r_symlink(struct load_arg *arg)
        /// </summary>
        /// <returns></returns>
        public RubySymbol ReadSymbolLink()
        {
            var num = ReadLong();
            if (num >= m_symbols.Count) throw new InvalidDataException("bad symbol");
            return m_symbols[num];
        }

        /// <summary>
        /// static ID r_symreal(struct load_arg *arg, int ivar)
        /// </summary>
        /// <param name="ivar"></param>
        /// <returns></returns>
        public RubySymbol ReadSymbolReal(bool ivar)
        {
            var s = ReadBytes();
            var n = m_symbols.Count;
            RubySymbol id;
            var idx = Encoding.UTF8;
            m_symbols.Add(n, null);
            if (ivar)
            {
                var num = ReadLong();
                while (num-- > 0)
                {
                    id = ReadSymbol();
                    idx = GetEncoding(id, ReadObject());
                }
            }
            var str = new RubyString(s, idx);
            id = RubySymbol.GetSymbol(str);
            m_symbols[n] = id;
            return id;
        }

        /// <summary>
        /// static int id2encidx(ID id, VALUE val)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public Encoding GetEncoding(RubySymbol id, object val)
        {
            if (id == RubyMarshal.IDs.encoding) return Encoding.GetEncoding(((RubyString) val).Text);
            else if (id == RubyMarshal.IDs.E)
            {
                if ((val is bool) && ((bool) val == false)) return Encoding.Default;
                if ((val is bool) && ((bool) val == true)) return Encoding.UTF8;
            }
            return null;
        }

        /// <summary>
        /// static ID r_symbol(struct load_arg *arg)
        /// </summary>
        /// <returns></returns>
        public RubySymbol ReadSymbol()
        {
            int type;
            var ivar = false;
            again:
            switch (type = ReadByte())
            {
                case RubyMarshal.Types.InstanceVariable:
                    ivar = true;
                    goto again;
                case RubyMarshal.Types.Symbol:
                    return ReadSymbolReal(ivar);
                case RubyMarshal.Types.SymbolLink:
                    if (ivar) throw new InvalidDataException("dump format error (symlink with encoding)");
                    return ReadSymbolLink();
                default:
                    throw new InvalidDataException($"dump format error for symbol(0x{type:X2})");
            }
        }

        /// <summary>
        /// static VALUE r_unique(struct load_arg *arg)
        /// </summary>
        /// <returns></returns>
        public RubySymbol ReadUnique() { return ReadSymbol(); }

        /// <summary>
        /// static VALUE r_string(struct load_arg *arg)
        /// </summary>
        /// <returns></returns>
        public RubyString ReadString()
        {
            var raw = ReadBytes();
            var v = new RubyString(raw);
            // TODO: detecting encoding
            if ((raw.Length > 2) && (raw[0] == 120) && (raw[1] == 156))
            {
                v.Encoding = Encoding.Default;
                // special treatment for zlib
            }
            else v.Encoding = Encoding.UTF8;
            return v;
        }

        /// <summary>
        /// static st_index_t r_prepare(struct load_arg *arg)
        /// </summary>
        /// <returns></returns>
        public int Prepare()
        {
            var idx = m_objects.Count;
            m_objects.Add(idx, null);
            return idx;
        }

        /// <summary>
        /// static VALUE r_entry0(VALUE v, st_index_t num, struct load_arg *arg)
        /// </summary>
        /// <param name="v"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public object Entry0(object v, int num)
        {
            object real_obj = null;
            if (m_compat_tbl.TryGetValue(v, out real_obj))
            {
                if (m_objects.ContainsKey(num)) m_objects[num] = real_obj;
                else m_objects.Add(num, real_obj);
            }
            else
            {
                if (m_objects.ContainsKey(num)) m_objects[num] = v;
                else m_objects.Add(num, v);
            }
            return v;
        }

        /// <summary>
        /// #define r_entry(v, arg) r_entry0((v), (arg)->data->num_entries, (arg))
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public object Entry(object v) { return Entry0(v, m_objects.Count); }

        /// <summary>
        /// static VALUE r_leave(VALUE v, struct load_arg *arg)
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public object Leave(object v)
        {
            if (m_compat_tbl.TryGetValue(v, out object data))
            {
                var real_obj = data;
                var key = v;
                // TODO: 实现 MarshalCompat
                // if (st_lookup(compat_allocator_tbl, (st_data_t)allocator, &data)) {
                //   marshal_compat_t *compat = (marshal_compat_t*)data;
                //   compat->loader(real_obj, v);
                // }
                m_compat_tbl.Remove(key);
                v = real_obj;
            }
            if (m_proc != null) { v = m_proc(v); }
            return v;
        }

        /// <summary>
        /// static void r_ivar(VALUE obj, int *has_encoding, struct load_arg *arg)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="has_encoding"></param>
        public void ReadInstanceVariable(object obj, ref bool has_encoding)
        {
            var len = ReadLong();
            var fobj = obj as RubyObject;
            if (len <= 0) return;
            do
            {
                var id = ReadSymbol();
                var val = ReadObject();
                var idx = GetEncoding(id, val);
                if (idx != null)
                {
                    if (fobj != null) fobj.Encoding = idx;
                    has_encoding = true;
                }
                else
                {
                    if (fobj != null) fobj.InstanceVariable[id] = val;
                }
            } while (--len > 0);
        }

        public void ReadInstanceVariable(object obj)
        {
            var e = false;
            ReadInstanceVariable(obj, ref e);
        }

        /// <summary>
        /// static VALUE append_extmod(VALUE obj, VALUE extmod)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="extmod"></param>
        /// <returns></returns>
        public object AppendExtendedModule(object obj, List<RubyModule> extmod)
        {
            if (obj is RubyObject fobj) fobj.ExtendModules.AddRange(extmod);
            return obj;
        }

        /// <summary>
        /// static VALUE r_object(struct load_arg *arg)
        /// </summary>
        /// <returns></returns>
        public object ReadObject()
        {
            var ivp = false;
            return ReadObject0(false, ref ivp, null);
        }

        public object ReadObject0(ref bool ivp, List<RubyModule> extmod) { return ReadObject0(true, ref ivp, extmod); }

        public object ReadObject0(List<RubyModule> extmod)
        {
            var ivp = false;
            return ReadObject0(false, ref ivp, extmod);
        }

        /// <summary>
        /// static VALUE r_object0(struct load_arg *arg, int *ivp, VALUE extmod)
        /// </summary>
        /// <param name="hasivp"></param>
        /// <param name="ivp"></param>
        /// <param name="extmod"></param>
        /// <returns></returns>
        public object ReadObject0(bool hasivp, ref bool ivp, List<RubyModule> extmod)
        {
            object v = null;
            var type = ReadByte();
            switch (type)
            {
                case RubyMarshal.Types.Link:
                    var id = ReadLong();
                    object link;
                    if (!m_objects.TryGetValue(id, out link)) throw new InvalidDataException("dump format error (unlinked)");
                    v = link;
                    if (m_proc != null) v = m_proc(v);
                    break;
                case RubyMarshal.Types.InstanceVariable:
                {
                    var ivar = true;
                    v = ReadObject0(ref ivar, extmod);
                    var hasenc = false;
                    if (ivar) ReadInstanceVariable(v, ref hasenc);
                }
                    break;
                case RubyMarshal.Types.Extended:
                {
                    var m = RubyModule.GetModule(ReadUnique());
                    if (extmod == null) extmod = new List<RubyModule>();
                    extmod.Add(m);
                    v = ReadObject0(extmod);
                    if (v is RubyObject fobj) fobj.ExtendModules.AddRange(extmod);
                }
                    break;
                case RubyMarshal.Types.UserClass:
                {
                    var c = RubyClass.GetClass(ReadUnique());
                    v = ReadObject0(extmod);
                    if (v is RubyObject) (v as RubyObject).ClassName = c.Symbol;
                }
                    break;
                case RubyMarshal.Types.Nil:
                    v = RubyNil.Instance;
                    v = Leave(v);
                    break;
                case RubyMarshal.Types.True:
                    v = RubyBool.True;
                    v = Leave(v);
                    break;
                case RubyMarshal.Types.False:
                    v = RubyBool.False;
                    v = Leave(v);
                    break;
                case RubyMarshal.Types.Fixnum:
                    v = ReadLong();
                    v = new RubyFixnum(Convert.ToInt64(v));
                    v = Leave(v);
                    break;
                case RubyMarshal.Types.Float:
                {
                    double d;
                    var fstr = ReadString();
                    var str = fstr.Text;

                    switch (str)
                    {
                        case "inf":
                            d = double.PositiveInfinity;
                            break;
                        case "-inf":
                            d = double.NegativeInfinity;
                            break;
                        case "nan":
                            d = double.NaN;
                            break;
                        default:
                            if (str.Contains("\0")) str = str.Remove(str.IndexOf("\0", StringComparison.Ordinal));

                            d = Convert.ToDouble(str);
                            break;
                    }
                    v = new RubyFloat(d);
                    v = Entry(v);
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.Bignum:
                {
                    var sign = 0;
                    switch (ReadByte())
                    {
                        case 0x2b:
                            sign = 1;
                            break;

                        case 0x2d:
                            sign = -1;
                            break;

                        default:
                            sign = 0;
                            break;
                    }
                    var num3 = ReadLong();
                    var index = num3 / 2;
                    var num5 = (num3 + 1) / 2;
                    var data = new uint[num5];
                    for (var i = 0; i < index; i++) { data[i] = m_reader.ReadUInt32(); }
                    if (index != num5) { data[index] = m_reader.ReadUInt16(); }
                    v = new RubyBignum(sign, data);
                    v = Entry(v);
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.String:
                    v = Entry(ReadString());
                    v = Leave(v);
                    break;
                case RubyMarshal.Types.Regexp:
                {
                    var str = ReadString();
                    var options = ReadByte();
                    var has_encoding = false;
                    var idx = Prepare();
                    if (hasivp)
                    {
                        ReadInstanceVariable(str, ref has_encoding);
                        ivp = false;
                    }
                    if (!has_encoding)
                    {
                        // TODO: 1.8 compatibility; remove escapes undefined in 1.8
                        /*
                        char *ptr = RSTRING_PTR(str), *dst = ptr, *src = ptr;
                        long len = RSTRING_LEN(str);
                        long bs = 0;
                        for (; len-- > 0; *dst++ = *src++) {
                            switch (*src) {
                                case '\\': bs++; break;
                                case 'g': case 'h': case 'i': case 'j': case 'k': case 'l':
                                case 'm': case 'o': case 'p': case 'q': case 'u': case 'y':
                                case 'E': case 'F': case 'H': case 'I': case 'J': case 'K':
                                case 'L': case 'N': case 'O': case 'P': case 'Q': case 'R':
                                case 'S': case 'T': case 'U': case 'V': case 'X': case 'Y':
                                if (bs & 1) --dst;
                                default: bs = 0; break;
                            }
                        }
                        rb_str_set_len(str, dst - ptr);
                        */
                    }
                    v = Entry0(new RubyRegexp(str, (RubyRegexpOptions) options), idx);
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.Array:
                {
                    var len = ReadLong();
                    var ary = new RubyArray();
                    v = ary;
                    v = Entry(v);
                    while (len-- > 0) ary.Push(ReadObject());
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.Hash:
                case RubyMarshal.Types.HashWithDefault:
                {
                    var len = ReadLong();
                    var hash = new RubyHash();
                    v = hash;
                    v = Entry(v);
                    while (len-- > 0)
                    {
                        var key = ReadObject();
                        var value = ReadObject();
                        hash.Add(key, value);
                    }
                    if (type == RubyMarshal.Types.HashWithDefault) hash.DefaultValue = ReadObject();
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.Struct:
                {
                    var idx = Prepare();
                    var obj = new RubyStruct();
                    var klass = ReadUnique();
                    obj.ClassName = klass;
                    var len = ReadLong();
                    v = obj;
                    v = Entry0(v, idx);
                    while (len-- > 0)
                    {
                        var key = ReadSymbol();
                        var value = ReadObject();
                        obj.InstanceVariable[key] = value;
                    }
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.UserDefined:
                {
                    var klass = ReadUnique();
                    var obj = RubyUserDefinedObject.TryGetUserDefinedObject(klass.Name);
                    if (obj == null)
                    {
                        var data = ReadString();
                        if (hasivp)
                        {
                            ReadInstanceVariable(data);
                            ivp = false;
                        }
                        obj = new DefaultRubyUserDefinedDumpObject {Raw = data.Raw};
                    }
                    else obj.Read(m_reader);
                    obj.ClassName = klass;
                    v = obj;
                    v = Entry(v);
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.UserMarshal:
                {
                    var klass = ReadUnique();
                    var obj = new DefaultRubyUserDefinedMarshalDumpObject();
                    v = obj;
                    if (extmod != null) AppendExtendedModule(obj, extmod);
                    v = Entry(v);
                    var data = ReadObject();
                    obj.ClassName = klass;
                    obj.DumpedObject = data;
                    v = Leave(v);
                    extmod?.Clear();
                }
                    break;
                case RubyMarshal.Types.Object:
                {
                    var idx = Prepare();
                    var obj = new RubyObject();
                    var klass = ReadUnique();
                    obj.ClassName = klass;
                    v = obj;
                    v = Entry0(v, idx);
                    ReadInstanceVariable(v);
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.Class:
                {
                    var str = ReadString();
                    v = RubyClass.GetClass(RubySymbol.GetSymbol(str));
                    v = Entry(v);
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.Module:
                {
                    var str = ReadString();
                    v = RubyModule.GetModule(RubySymbol.GetSymbol(str));
                    v = Entry(v);
                    v = Leave(v);
                }
                    break;
                case RubyMarshal.Types.Symbol:
                    if (hasivp)
                    {
                        v = ReadSymbolReal(ivp);
                        ivp = false;
                    }
                    else v = ReadSymbolReal(false);
                    v = Leave(v);
                    break;
                case RubyMarshal.Types.SymbolLink:
                    v = ReadSymbolLink();
                    break;
                case RubyMarshal.Types.Data:
                /*  TODO: Data Support
                    {
                        VALUE klass = path2class(r_unique(arg));
                        VALUE oldclass = 0;

                        v = obj_alloc_by_klass(klass, arg, &oldclass);
                        if (!RB_TYPE_P(v, T_DATA)) {
                            rb_raise(rb_eArgError, "dump format error");
                        }
                        v = r_entry(v, arg);
                        if (!rb_respond_to(v, s_load_data)) {
                            rb_raise(rb_eTypeError, "class %s needs to have instance method `_load_data'", rb_class2name(klass));
                        }
                        rb_funcall(v, s_load_data, 1, r_object0(arg, 0, extmod));
                        check_load_arg(arg, s_load_data);
                        v = r_leave(v, arg);
                    }
                 */
                case RubyMarshal.Types.ModuleOld:
                /*
                    TODO: ModuleOld Support
                    {
                        volatile VALUE str = r_bytes(arg);
                        v = rb_path_to_class(str);
                        v = r_entry(v, arg);
                        v = r_leave(v, arg);
                    }
                 */
                default:
                    throw new InvalidDataException($"dump format error(0x{type:X2})");
            }
            return v;
        }
    }
}