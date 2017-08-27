using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IamI.Lib.Serialization.RubyMarshal.OriginModel;

namespace IamI.Lib.Serialization.RubyMarshal
{
    public static class NativeModelTransformHelper
    {
        public static object TransformToNativeModel(object obj)
        {
            if (obj is RubyObject robj) return robj.TransformToNativeModel();
            return obj;
        }

        public static IEnumerable<object> TransformToNativeModel(this RubyArray self) { return self.list.Select(TransformToNativeModel); }

        public static object TransformToNativeModel(this RubyBignum self) { return self; }

        public static bool TransformToNativeModel(this RubyBool self) { return self.Value; }

        public static object TransformToNativeModel(this RubyClass self) { return self; }

        public static long TransformToNativeModel(this RubyFixnum self) { return self.Value; }

        public static double TransformToNativeModel(this RubyFloat self) { return self.Value; }

        public static object TransformToNativeModel(this RubyHash self) { return self.value.ToDictionary(pair => TransformToNativeModel(pair.Key), pair => TransformToNativeModel(pair.Value)); }

        public static object TransformToNativeModel(this RubyModule self) { return self; }

        public static object TransformToNativeModel(this RubyNil self) { return null; }

        public static object TransformToNativeModel(this RubyObject self)
        {
            object result = null;
            if (self is RubyArray ruby_array) result = ruby_array.TransformToNativeModel();
            else if (self is RubyBignum ruby_bignum) result = ruby_bignum.TransformToNativeModel();
            else if (self is RubyBool ruby_bool) result = ruby_bool.TransformToNativeModel();
            else if (self is RubyClass ruby_class) result = ruby_class.TransformToNativeModel();
            else if (self is RubyFixnum ruby_fixnum) result = ruby_fixnum.TransformToNativeModel();
            else if (self is RubyFloat ruby_float) result = ruby_float.TransformToNativeModel();
            else if (self is RubyHash ruby_hash) result = ruby_hash.TransformToNativeModel();
            else if (self is RubyNil ruby_nil) result = ruby_nil.TransformToNativeModel();
            else if (self is RubyRegexp ruby_regexp) result = ruby_regexp.TransformToNativeModel();
            else if (self is RubyString ruby_string) result = ruby_string.TransformToNativeModel();
            else if (self is RubyStruct ruby_struct) result = ruby_struct.TransformToNativeModel();
            else if (self is RubySymbol ruby_symbol) result = ruby_symbol.TransformToNativeModel();
            else if (self is RubyUserDefinedObject ruby_user_defined) result = ruby_user_defined.TransformToNativeModel();
            return result;
        }

        public static Regex TransformToNativeModel(this RubyRegexp self)
        {
            var option = RegexOptions.None;
            switch (self.Options)
            {
                case RubyRegexpOptions.None:
                    // Ignored.
                    break;
                case RubyRegexpOptions.IgnoreCase:
                    option |= RegexOptions.IgnoreCase;
                    break;
                case RubyRegexpOptions.Extend:
                    option |= RegexOptions.ExplicitCapture;
                    break;
                case RubyRegexpOptions.Multiline:
                    option |= RegexOptions.Multiline;
                    break;
                case RubyRegexpOptions.Singleline:
                    option |= RegexOptions.Singleline;
                    break;
                case RubyRegexpOptions.FindLongest:
                    // Ignored.
                    break;
                case RubyRegexpOptions.FindNotEmpty:
                    option |= RegexOptions.IgnorePatternWhitespace;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new Regex(self.Pattern.Text, option);
        }

        public static string TransformToNativeModel(this RubyString self) { return self.Text; }

        public static object TransformToNativeModel(this RubyStruct self) { return self; }

        public static string TransformToNativeModel(this RubySymbol self) { return self.Name; }
    }
}