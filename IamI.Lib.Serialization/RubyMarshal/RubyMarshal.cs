using System;
using System.IO;
using IamI.Lib.Serialization.RubyMarshal.OriginModel;

namespace IamI.Lib.Serialization.RubyMarshal
{
    public static class RubyMarshal
    {
        /// <summary>
        ///     控制采用旧式还是新式的 RubyString 写入方式。
        /// </summary>
        [Serializable]
        public enum StringStyleType
        {
            /// <summary>
            ///     使用 Ruby 1.8.6 之前，Marshal 4.8 的 RubyString 写入方式。
            /// </summary>
            Style18,

            /// <summary>
            ///     使用 Ruby 1.9.2 之后，Marshal 4.8 的 RubyString 写入方式。
            ///     更早版本的 Ruby 可能不能正确读取以此法写入的文件。
            /// </summary>
            Style19
        }

        /// <summary>
        ///     当前 RubyMarshal 的主版本号。
        /// </summary>
        public const byte MARSHAL_MAJOR = 4;

        /// <summary>
        ///     当前 RubyMarshal 的副版本号。
        /// </summary>
        public const byte MARSHAL_MINOR = 8;

        /// <summary>
        ///     从目标流中读取保存的对象。
        /// </summary>
        /// <param name="input">目标输入流</param>
        /// <returns>流中保存的对象</returns>
        public static object Load(Stream input)
        {
            var reader = new RubyMarshalReader(input);
            return reader.Load();
        }

        /// <summary>
        ///     向目标流中写入序列化的 param。
        /// </summary>
        /// <param name="output">目标输出流</param>
        /// <param name="param">要写入的对象</param>
        /// <param name="string_style_type">指示应用何种方式写入 String 类别。</param>
        public static void Dump(Stream output, object param,
            StringStyleType string_style_type = StringStyleType.Style19)
        {
            var writer = new RubyMarshalWriter(output) {StringStyle = string_style_type};
            writer.Dump(param);
        }

        public abstract class Ds
        {
            /// <summary>
            ///     用以表示编码的固定 Magic Symbol。
            /// </summary>
            public static RubySymbol encoding = RubySymbol.GetSymbol("encoding");

            /// <summary>
            ///     用以表示 StringStyleType::Style19 的固定 Magic Symbol。
            /// </summary>
            public static RubySymbol e = RubySymbol.GetSymbol("E");
        }

        public abstract class Types
        {
            public const byte NIL = (byte) '0';
            public const byte TRUE = (byte) 'T';
            public const byte FALSE = (byte) 'F';
            public const byte FIXNUM = (byte) 'i';
            public const byte EXTENDED = (byte) 'e';
            public const byte USER_CLASS = (byte) 'C';
            public const byte OBJECT = (byte) 'o';
            public const byte DATA = (byte) 'd';
            public const byte USER_DEFINED = (byte) 'u';
            public const byte USER_MARSHAL = (byte) 'U';
            public const byte FLOAT = (byte) 'f';
            public const byte BIGNUM = (byte) 'l';
            public const byte STRING = (byte) '"';
            public const byte REGEXP = (byte) '/';
            public const byte ARRAY = (byte) '[';
            public const byte HASH = (byte) '{';
            public const byte HASH_WITH_DEFAULT = (byte) '}';
            public const byte STRUCT = (byte) 'S';
            public const byte MODULE_OLD = (byte) 'M';
            public const byte CLASS = (byte) 'c';
            public const byte MODULE = (byte) 'm';
            public const byte SYMBOL = (byte) ':';
            public const byte SYMBOL_LINK = (byte) ';';
            public const byte INSTANCE_VARIABLE = (byte) 'I';
            public const byte LINK = (byte) '@';
        }
    }
}