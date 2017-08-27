using System;
using System.Drawing;
using System.IO;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel.RPGMakerUserDefinedClass
{
    public class RubyRectangle : RubyUserDefinedObject
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        
        public override object TransformToNativeModel() { return new Rectangle(Convert.ToInt32(Left), Convert.ToInt32(Top), Convert.ToInt32(Width), Convert.ToInt32(Height)); }
        
        public override RubyUserDefinedObject Read(BinaryReader reader)
        {
            reader.ReadByte(); // LENGTH = 37
            Left = reader.ReadDouble();
            Top = reader.ReadDouble();
            Width = reader.ReadDouble();
            Height = reader.ReadDouble();
            return this;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte) 37);
            writer.Write(Left);
            writer.Write(Top);
            writer.Write(Width);
            writer.Write(Height);
        }
        
        static RubyRectangle() { RegisterUserDefinedType(typeof(RubyRectangle), "rectangle"); }
    }

}