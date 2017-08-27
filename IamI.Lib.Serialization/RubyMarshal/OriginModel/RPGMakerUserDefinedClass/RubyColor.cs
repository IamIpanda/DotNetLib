using System;
using System.Drawing;
using System.IO;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel.RPGMakerUserDefinedClass
{
    public class RubyColor : RubyUserDefinedObject
    {
        public double Red { get; set; }
        public double Green { get; set; }
        public double Blue { get; set; }
        public double Alpha { get; set; }

        public override object TransformToNativeModel() { return Color.FromArgb(Convert.ToInt32(Alpha), Convert.ToInt32(Red), Convert.ToInt32(Green), Convert.ToInt32(Blue)); }

        public override RubyUserDefinedObject Read(BinaryReader reader)
        {
            reader.ReadByte(); // LENGTH = 37
            Red = reader.ReadDouble();
            Green = reader.ReadDouble();
            Blue = reader.ReadDouble();
            Alpha = reader.ReadDouble();
            return this;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte) 37);
            writer.Write(Red);
            writer.Write(Green);
            writer.Write(Blue);
            writer.Write(Alpha);
        }

        static RubyColor() { RegisterUserDefinedType(typeof(RubyColor), "color"); }
    }
}