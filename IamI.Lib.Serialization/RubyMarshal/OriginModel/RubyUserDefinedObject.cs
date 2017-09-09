using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IamI.Lib.Basic.Log;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    public abstract class RubyUserDefinedObject : RubyObject
    {
        public abstract RubyUserDefinedObject Read(BinaryReader reader);
        public abstract void Write(BinaryWriter writer);
        public virtual object TransformToNativeModel() { return this; }

        private static readonly Dictionary<string, Type> user_defined_types = new Dictionary<string, Type>();

        protected static void RegisterUserDefinedType(Type type, string name = null)
        {
            if (name == null) name = type.Name;
            if (user_defined_types.ContainsKey(name))
            {
                Logger.Default.Warning($"Trying to override register ruby marshal user defined object: [{name}] from {user_defined_types[name].FullName} to {type.FullName}");
                user_defined_types[name] = type;
            }
            else
            {
                Logger.Default.Debug($"Registered ruby marshal user defined object: [{name}] {type.FullName}");
                user_defined_types.Add(name, type);
            }
        }

        /// <summary>
        /// 通过反射扫描目标程序集，将其中包含有 RubyUserDefinedObjectAttribute 特性的类均予以注册。
        /// </summary>
        /// <param name="assembly">要扫描的目标程序集。</param>
        public static void RegisterUserDefinedInAssembly(Assembly assembly)
        {
            foreach (var type in assembly.ExportedTypes.Where(type => Attribute.IsDefined(type, typeof(RubyUserDefinedObjectAttribute))))
                // For each scanned type, Register it to UserDefinedTypes.
                RegisterUserDefinedType(type, ((RubyUserDefinedObjectAttribute) Attribute.GetCustomAttribute(type, typeof(RubyUserDefinedObjectAttribute))).Keyword);
        }

        static RubyUserDefinedObject() { RegisterUserDefinedInAssembly(typeof(RubyUserDefinedObject).Assembly); }

        public static RubyUserDefinedObject TryGetUserDefinedObject(string type)
        {
            if (user_defined_types.ContainsKey(type)) return Activator.CreateInstance(user_defined_types[type]) as RubyUserDefinedObject;
            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RubyUserDefinedObjectAttribute : Attribute
    {
        public string Keyword { get; set; }

        public RubyUserDefinedObjectAttribute(string keyword) { Keyword = keyword; }
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