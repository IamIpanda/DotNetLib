using System;
using System.Collections.Generic;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("RubyMarshal::Hash, Count = {" + nameof(Count) + "}")]
    [System.Diagnostics.DebuggerTypeProxy(typeof(RubyHashDebugView))]
    public class RubyHash : RubyObject, IEnumerable<KeyValuePair<object, object>>
    {
        public readonly Dictionary<object, object> value = new Dictionary<object, object>();
        public RubyHash() : this(null) { }
        public object DefaultValue { get; set; }

        public RubyHash(object default_value)
        {
            DefaultValue = default_value;
            ClassName = RubySymbol.GetSymbol("Hash");
        }

        public object this[object key] { get { return value.TryGetValue(key, out object result) ? result : DefaultValue; } set { this.value[key] = value; } }

        public IEqualityComparer<object> Comparer => value.Comparer;
        public int Count => value.Count;
        public int Size => value.Count;
        public int Length => value.Count;
        public Dictionary<object, object>.KeyCollection Keys => value.Keys;
        public Dictionary<object, object>.ValueCollection Values => value.Values;
        public void Add(object key, object value) { this.value.Add(key, value); }
        public void Clear() { value.Clear(); }
        public bool ContainsKey(object key) { return value.ContainsKey(key); }
        public bool ContainsValue(object value) { return this.value.ContainsValue(value); }
        public Dictionary<object, object>.Enumerator GetEnumerator() { return value.GetEnumerator(); }
        public bool Remove(object key) { return value.Remove(key); }
        public bool TryGetValue(object key, out object value) { return this.value.TryGetValue(key, out value); }

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator() { return value.GetEnumerator(); }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return value.GetEnumerator(); }

        internal class RubyHashDebugView
        {
            private readonly RubyHash _hashtable;
            public RubyHashDebugView(RubyHash hashtable) { this._hashtable = hashtable; }

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<object, object>[] Keys
            {
                get
                {
                    var keys = new KeyValuePair<object, object>[_hashtable.Count];

                    var i = 0;
                    foreach (var key in _hashtable)
                    {
                        keys[i] = key;
                        i++;
                    }
                    return keys;
                }
            }
        }

        public override object Clone()
        {
            var clone_target = new RubyHash(DefaultValue);
            foreach (var key in clone_target.Keys) clone_target.Add(Dup(key), Dup(clone_target[key]));
            return clone_target;
        }
    }
}