using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace AngularParserCSharp
{
    public class JavascriptObject : DynamicObject, IDictionary<string, object>
    {
        #region Fields

        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

        #endregion

        #region Properties

        public static JavascriptObject Undefined { get; } = new UndefinedObject();

        public int Count => _values.Count;
        public bool IsReadOnly => _values.IsReadOnly;

        public ICollection<string> Keys => _values.Keys;
        public ICollection<object> Values => _values.Values;

        public virtual object this[string key]
        {
            get => _values.TryGetValue(key, out var result) ? result : Undefined;
            set => _values[key] = value;
        }

        #endregion

        #region  Public Methods

        public virtual void Add(KeyValuePair<string, object> item)
        {
            _values.Add(item);
        }

        public virtual void Add(string key, object value)
        {
            _values.Add(key, value);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _values.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _values.Remove(item);
        }

        public bool Remove(string key)
        {
            return _values.Remove(key);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!_values.TryGetValue(binder.Name, out result))
            {
                result = Undefined;
            }
            return true;
        }

        public bool TryGetValue(string key, out object value)
        {
            return _values.TryGetValue(key, out value);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _values[binder.Name] = value;
            return true;
        }

        #endregion

        #region  Nonpublic Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}