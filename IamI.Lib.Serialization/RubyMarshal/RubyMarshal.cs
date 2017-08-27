using System;
using System.IO;
using IamI.Lib.Serialization.RubyMarshal.OriginModel;

namespace IamI.Lib.Serialization.RubyMarshal
{
    public static class RubyMarshal
    {
        /// <summary>
        /// 当前 RubyMarshal 的主版本号。
        /// </summary>
        public const byte MarshalMajor = 4;
        
        /// <summary>
        /// 当前 RubyMarshal 的副版本号。
        /// </summary>
        public const byte MarshalMinor = 8;

        public abstract class IDs
        {
            /// <summary>
            /// 用以表示编码的固定 Magic Symbol。
            /// </summary>
            public static RubySymbol encoding = RubySymbol.GetSymbol("encoding");
            
            /// <summary>
            /// 用以表示 StringStyleType::Style19 的固定 Magic Symbol。
            /// </summary>
            public static RubySymbol E = RubySymbol.GetSymbol("E");
        }

        public abstract class Types
        {
            public const byte Nil = (byte) '0';
            public const byte True = (byte) 'T';
            public const byte False = (byte) 'F';
            public const byte Fixnum = (byte) 'i';
            public const byte Extended = (byte) 'e';
            public const byte UserClass = (byte) 'C';
            public const byte Object = (byte) 'o';
            public const byte Data = (byte) 'd';
            public const byte UserDefined = (byte) 'u';
            public const byte UserMarshal = (byte) 'U';
            public const byte Float = (byte) 'f';
            public const byte Bignum = (byte) 'l';
            public const byte String = (byte) '"';
            public const byte Regexp = (byte) '/';
            public const byte Array = (byte) '[';
            public const byte Hash = (byte) '{';
            public const byte HashWithDefault = (byte) '}';
            public const byte Struct = (byte) 'S';
            public const byte ModuleOld = (byte) 'M';
            public const byte Class = (byte) 'c';
            public const byte Module = (byte) 'm';
            public const byte Symbol = (byte) ':';
            public const byte SymbolLink = (byte) ';';
            public const byte InstanceVariable = (byte) 'I';
            public const byte Link = (byte) '@';
        }

        /// <summary>
        /// 从目标流中读取保存的对象。
        /// </summary>
        /// <param name="input">目标输入流</param>
        /// <returns>流中保存的对象</returns>
        public static object Load(Stream input)
        {
            var reader = new RubyMarshalReader(input);
            return reader.Load();
        }

        /// <summary>
        /// 向目标流中写入序列化的 param。
        /// </summary>
        /// <param name="output">目标输出流</param>
        /// <param name="param">要写入的对象</param>
        public static void Dump(Stream output, object param)
        {
            var writer = new RubyMarshalWriter(output);
            writer.Dump(param);
        }

        /// <summary>
        /// 控制采用旧式还是新式的 RubyString 写入方式。
        /// </summary>
        [Serializable]
        public enum StringStyleType
        {
            /// <summary>
            /// 使用 Ruby 1.8.6 之前，Marshal 4.8 的 RubyString 写入方式。
            /// </summary>
            Style18,
            /// <summary>
            /// 使用 Ruby 1.9.2 之后，Marshal 4.8 的 RubyString 写入方式。
            /// 更早版本的 Ruby 可能不能正确读取以此法写入的文件。
            /// </summary>
            Style19
        };
    }
}