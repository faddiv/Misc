using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AngularParserCSharp
{
    public class UndefinedObject : JavascriptObject
    {
        #region Properties

        public override object this[string key]
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        #endregion

        #region  Public Methods

        public override void Add(KeyValuePair<string, object> item)
        {
            throw new InvalidOperationException();
        }

        public override void Add(string key, object value)
        {
            throw new InvalidOperationException();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return false;
        }

        #endregion
    }
}