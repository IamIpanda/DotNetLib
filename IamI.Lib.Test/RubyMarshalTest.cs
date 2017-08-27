using System.IO;
using System.Linq.Expressions;
using IamI.Lib.Serialization.RubyMarshal;
using IamI.Lib.Serialization.RubyMarshal.OriginModel.RPGMakerUserDefinedClass;

namespace IamI.Lib.Test
{
    public static class RubyMarshalTest
    {
        public static void Test1()
        {
            new RubyTable();
            new RubyColor();
            new RubyTone();
            new RubyRectangle();
            var obj = RubyMarshal.Load(new FileStream("Data/System.rvdata2", FileMode.Open));
        }
    }
}