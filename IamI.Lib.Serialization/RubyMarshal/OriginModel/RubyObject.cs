using System;
using System.Collections.Generic;
using System.Text;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [System.Diagnostics.DebuggerTypeProxy(typeof(RubyObjectDebugView))]
    [Serializable]
    public class RubyObject : ICloneable
    {
        public virtual RubyClass Class => RubyClass.GetClass(ClassName);
        public List<RubyModule> ExtendModules { get; set; } = new List<RubyModule>();
        public Dictionary<RubySymbol, object> InstanceVariables { get; } = new Dictionary<RubySymbol, object>();
        public RubyObjectInstanceVariableProxy InstanceVariable { get; private set; }

        public RubyObject() { InstanceVariable = new RubyObjectInstanceVariableProxy(this); }

        protected RubySymbol class_name;
        public virtual RubySymbol ClassName { get { return class_name ?? (class_name = RubySymbol.GetSymbol("Object")); } set { class_name = value; } }

        public override string ToString() { return "#<" + ClassName + ">"; }

        public virtual Encoding Encoding { get; set; }

        protected object Dup(object origin)
        {
            var cloneable = origin as ICloneable;
            return cloneable?.Clone() ?? origin;
        }

        public virtual object Clone()
        {
            var clone_object = new RubyObject {ClassName = ClassName};
            foreach (var key in InstanceVariables.Keys) clone_object.InstanceVariables.Add(key, Dup(InstanceVariables[key]));
            return clone_object;
        }

        [Serializable]
        internal class RubyObjectDebugView
        {
            internal RubyObject obj;

            public RubyObjectDebugView(RubyObject obj) { this.obj = obj; }

            public RubySymbol ClassName => obj.ClassName;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<RubySymbol, object>[] Keys
            {
                get
                {
                    var keys = new KeyValuePair<RubySymbol, object>[obj.InstanceVariables.Count];
                    var i = 0;
                    foreach (var key in obj.InstanceVariables)
                    {
                        keys[i] = key;
                        i++;
                    }
                    return keys;
                }
            }
        }

        [Serializable]
        public class RubyObjectInstanceVariableProxy
        {
            private readonly RubyObject obj;
            internal RubyObjectInstanceVariableProxy(RubyObject obj) { this.obj = obj; }

            public object this[RubySymbol key]
            {
                get { return obj.InstanceVariables.ContainsKey(key) ? obj.InstanceVariables[key] is RubyNil ? null : obj.InstanceVariables[key] : null; }
                set
                {
                    if (obj.InstanceVariables.ContainsKey(key)) obj.InstanceVariables[key] = value;
                    else obj.InstanceVariables.Add(key, value);
                }
            }

            public object this[string key] { get { return this[RubySymbol.GetSymbol(key)]; } set { this[RubySymbol.GetSymbol(key)] = value; } }

            public object this[RubyString key] { get { return this[RubySymbol.GetSymbol(key)]; } set { this[RubySymbol.GetSymbol(key)] = value; } }
        }
    }
}