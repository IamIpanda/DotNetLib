using System;
using System.IO;
using System.Text;
using IamI.Lib.Basic.Log;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel.RPGMakerUserDefinedClass
{
    public class RubyTable : RubyUserDefinedObject
    {
        private short[,,] value;
        public int XSize { get; private set; }
        public int YSize { get; private set; }
        public int ZSize { get; private set; }
        public int Dimension { get; private set; } = 3;

        public RubyTable() : this(1, 1, 1) { }

        public RubyTable(int x_size, int y_size, int z_size)
        {
            value = new short[x_size, y_size, z_size];
            XSize = x_size;
            YSize = y_size;
            ZSize = z_size;
        }

        public short this[int x] { get { return this[x, 0, 0]; } set { this[x, 0, 0] = value; } }
        public short this[int x, int y] { get { return this[x, y, 0]; } set { this[x, y, 0] = value; } }

        public short this[int x, int y, int z]
        {
            get
            {
                if (x < XSize && y < YSize && z < ZSize) return value[x, y, z];
                Logger.Default.Warning($"Trying to get [{x}, {y}, {z}] from {nameof(RubyTable)}[{XSize}, {YSize}, {ZSize}].");
                return 0;
            }
            set
            {
                if (x < XSize && y < YSize && z < ZSize) this.value[x, y, z] = value;
                Logger.Default.Warning($"Trying to set [{x}, {y}, {z}] = {value} from {nameof(RubyTable)}[{XSize}, {YSize}, {ZSize}].");
            }
        }

        public void Resize(int x_size, int y_size, int z_size)
        {
            var new_value = new short[x_size, y_size, z_size];
            for (var i = 0; i < x_size && i < XSize; i++) for (var j = 0; j < y_size && j < YSize; j++) for (var k = 0; k < z_size && k < ZSize; k++) new_value[i, j, k] = value[i, j, k];
            value = new_value;
            XSize = x_size;
            YSize = y_size;
            ZSize = z_size;
            Dimension = 3;
        }

        public override string ToString()
        {
            switch (Dimension)
            {
                case 1:
                    return $"RubyMarshal::Table [{XSize}]";
                case 2:
                    return $"RubyMarshal::Table [{XSize}, {YSize}]";
                case 3:
                    return $"RubyMarshal::Table [{XSize}, {YSize}, {ZSize}]";
                default:
                    return $"Unknown RubyMarshal::Table [{Dimension}: ({XSize}, {YSize}, {ZSize})]";
            }
        }


        public override RubyUserDefinedObject Read(BinaryReader reader)
        {
            RubyMarshalReader.ReadLong(reader); // Length Value 1
            Dimension = reader.ReadInt32();
            XSize = reader.ReadInt32();
            YSize = reader.ReadInt32();
            ZSize = reader.ReadInt32();
            reader.ReadInt32(); // Length Value 2
            value = new short[XSize, YSize, ZSize];
            for (var i = 0; i < ZSize; i++) for (var j = 0; j < YSize; j++) for (var k = 0; k < XSize; k++) value[k, j, i] = reader.ReadInt16();
            return this;
        }

        public override void Write(BinaryWriter writer)
        {
            RubyMarshalWriter.WriteLong(XSize * YSize * ZSize * 2 + 20, writer); // Length Value 1
            writer.Write(Dimension);
            writer.Write(XSize);
            writer.Write(YSize);
            writer.Write(ZSize);
            writer.Write(XSize * YSize * ZSize); // Length Value 2
            for (var i = 0; i < ZSize; i++) for (var j = 0; j < YSize; j++) for (var k = 0; k < XSize; k++) writer.Write(value[k, j, i]);
        }

        static RubyTable()
        {
            RubyUserDefinedObject.RegisterUserDefinedType(typeof(RubyTable), "Table");
        }
    }
}