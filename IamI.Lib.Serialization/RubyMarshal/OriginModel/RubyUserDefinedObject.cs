using System;
using System.IO;
using System.Collections.Generic;
using IamI.Lib.Basic.Log;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    public abstract class RubyUserDefinedObject : RubyObject
    {
        public abstract RubyUserDefinedObject Read(BinaryReader reader);
        public abstract void Write(BinaryWriter writer);
        public virtual object TransformToNativeModel() { return this; }

        private static readonly Dictionary<string, Type> UserDefinedTypes = new Dictionary<string, Type>();

        protected static void RegisterUserDefinedType(Type type, string name = null)
        {
            if (name == null) name = type.Name;
            if (UserDefinedTypes.ContainsKey(name))
            {
                Logger.Default.Warning($"Trying to override register ruby marshal user defined object: {name} to {type.FullName}");
                UserDefinedTypes[name] = type;
            }
            else UserDefinedTypes.Add(name, type);
        }

        public static RubyUserDefinedObject TryGetUserDefinedObject(string type)
        {
            if (UserDefinedTypes.ContainsKey(type)) return Activator.CreateInstance(UserDefinedTypes[type]) as RubyUserDefinedObject;
            return null;
        }
    }

    public class DefaultRubyUserDefinedDumpObject : RubyUserDefinedObject
    {
        public byte[] Raw { get; set; }

        public override RubyUserDefinedObject Read(BinaryReader reader) { return this; }

        public override void Write(BinaryWriter writer) { }
    }

    public class DefaultRubyUserDefinedMarshalDumpObject : RubyUserDefinedObject
    {
        public object DumpedObject { get; set; }

        public override RubyUserDefinedObject Read(BinaryReader reader) { return this; }

        public override void Write(BinaryWriter writer) { }
    }
}