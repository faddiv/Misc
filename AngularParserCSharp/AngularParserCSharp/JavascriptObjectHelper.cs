using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AngularParserCSharp
{
    public class JavascriptObjectHelper
    {
        #region  Public Methods

        public bool IsTruthy(object value)
        {
            if (value == null) return false;
            if (ReferenceEquals(value, JavascriptObject.Undefined)) return false;
            if (Equals(value, Double.NaN)) return false;
            if (value is string) return !Equals(value, String.Empty);
            var type = value.GetType();
            if (type.IsValueType)
            {
                return Convert.ToBoolean(value);
            }
            return true;
        }

        public object SelectValue(object scope, object locals, object propertySelector)
        {
            if (IsTruthy(locals) && TryGetValue(locals, propertySelector, out var value))
            {
                return value;
            }
            if (IsTruthy(scope) && TryGetValue(scope, propertySelector, out value))
            {
                return value;
            }
            return JavascriptObject.Undefined;
        }

        public Delegate SelectFunction(object scope, object locals, string functionName)
        {
            if (TryGetFunction(locals, functionName, out var value)
                || TryGetFunction(scope, functionName, out value))
            {
                return value;
            }
            value = SelectValue(scope, locals, functionName) as Delegate;
            return value ?? new Func<object>(() => null);
        }

        private bool TryGetFunction(object target, string functionName, out Delegate value)
        {
            value = null;
            if (!IsTruthy(target)) return false;
            var methodInfo = target.GetType().GetMethod(functionName);
            if (methodInfo == null)
                return false;
            var parameters = (from parameter in methodInfo.GetParameters() select parameter.ParameterType)
                .Concat(new[] { methodInfo.ReturnType })
                .ToArray();
            value = methodInfo.CreateDelegate(Expression.GetDelegateType(parameters), target);
            return true;
        }

        #endregion

        #region  Nonpublic Methods

        private static bool TryGetValue(object obj, object propertySelector, out object value)
        {
            value = null;
            if (obj == null || obj == JavascriptObject.Undefined)
                return false;
            if (propertySelector == null || propertySelector == JavascriptObject.Undefined)
            {
                return false;
            }
            var objType = obj.GetType();
            if (objType.IsArray)
            {
                var index = Convert.ToInt32(propertySelector);
                if(!(obj is IList list)) throw new Exception($"Cant convert to list: {objType.Name}");
                value = list[index];
                return true;
            }
            var propertyName = Convert.ToString(propertySelector);
            if (obj is IDictionary<string, object> dvp)
                return dvp.TryGetValue(propertyName, out value);
            var property =
                TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>().FirstOrDefault(e => e.Name == propertyName);
            if (property == null)
                return false;
            value = property.GetValue(obj);
            return true;
        }

        #endregion

        public static MethodInfo IsTruthyInfo = typeof(JavascriptObjectHelper).GetMethod(nameof(IsTruthy));
        public static MethodInfo SelectValueInfo = typeof(JavascriptObjectHelper).GetMethod(nameof(SelectValue));
        public static MethodInfo SelectFunctionInfo = typeof(JavascriptObjectHelper).GetMethod(nameof(SelectFunction));
        
    }


    public class TypedJavascriptObjectHelper
    {
        #region  Public Methods

        public bool IsTruthy<T>(T value)
        {
            if (value == null) return false;
            if (ReferenceEquals(value, JavascriptObject.Undefined)) return false;
            if (Equals(value, Double.NaN)) return false;
            if (value is string) return !Equals(value, string.Empty);
            var type = value.GetType();
            return !type.IsValueType || Convert.ToBoolean(value);
        }

        public TReturn SelectValue<TScope, TReturn>(TScope scope, object locals, object propertySelector)
        {
            if (TryGetValue(locals, propertySelector, out var value)
                || TryGetValue(scope, propertySelector, out value))
            {
                return (TReturn)Convert.ChangeType(value, typeof(TReturn));
            }
            if (typeof(JavascriptObject) == typeof(TReturn))
            {
                return (TReturn) (object) JavascriptObject.Undefined;
            }
            else
            {
                return Activator.CreateInstance<TReturn>();
            }
        }

        public Delegate SelectFunction<T>(T scope, object locals, string functionName)
        {
            if (TryGetFunction(locals, functionName, out var value)
                || TryGetFunction(scope, functionName, out value))
            {
                return value;
            }
            value = SelectValue<T, Delegate>(scope, locals, functionName);
            return value ?? new Func<object>(() => null);
        }

        private bool TryGetFunction(object target, string functionName, out Delegate value)
        {
            value = null;
            if (!IsTruthy(target)) return false;
            var methodInfo = target.GetType().GetMethod(functionName);
            if (methodInfo == null)
                return false;
            var parameters = (from parameter in methodInfo.GetParameters() select parameter.ParameterType)
                .Concat(new[] { methodInfo.ReturnType })
                .ToArray();
            value = methodInfo.CreateDelegate(Expression.GetDelegateType(parameters), target);
            return true;
        }

        #endregion

        #region  Nonpublic Methods

        private bool TryGetValue(object obj, object propertySelector, out object value)
        {
            value = null;
            if (!IsTruthy(obj))
                return false;
            if (obj == null || obj == JavascriptObject.Undefined)
                return false;
            if (propertySelector == null || propertySelector == JavascriptObject.Undefined)
            {
                return false;
            }
            var objType = obj.GetType();
            if (objType.IsArray)
            {
                var index = Convert.ToInt32(propertySelector);
                if (!(obj is IList list)) throw new Exception($"Cant convert to list: {objType.Name}");
                value = list[index];
                return true;
            }
            var propertyName = Convert.ToString(propertySelector);
            if (obj is IDictionary<string, object> dvp)
                return dvp.TryGetValue(propertyName, out value);
            var property =
                TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>().FirstOrDefault(e => e.Name == propertyName);
            if (property == null)
                return false;
            value = property.GetValue(obj);
            return true;
        }

        #endregion

        public static MethodInfo IsTruthyInfo = typeof(TypedJavascriptObjectHelper).GetMethod(nameof(IsTruthy));
        public static MethodInfo SelectValueInfo = typeof(TypedJavascriptObjectHelper).GetMethod(nameof(SelectValue));
        public static MethodInfo SelectFunctionInfo = typeof(TypedJavascriptObjectHelper).GetMethod(nameof(SelectFunction));

    }
}