using System.IO;
using IamI.Lib.Serialization.RubyMarshal;

namespace IamI.Lib.Test
{
    public static class RubyMarshalTest
    {
        public static void Test1()
        {
            var obj = RubyMarshal.Load(new FileStream("Data/Actors.rvdata2", FileMode.Open));
            RubyMarshal.Dump(new FileStream("Data/Actors.rxdata2", FileMode.Create), obj);
        }
    }
}
