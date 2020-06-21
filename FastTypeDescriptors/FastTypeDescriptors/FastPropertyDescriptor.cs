using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace FastTypeDescriptors
{
    public class FastPropertyDescriptor : PropertyDescriptor
    {
        private Action<object, object> _setter;
        private Func<object, object> _getter;
        public FastPropertyDescriptor(Type componentType, Type propertyType, string propertyName, Attribute[] attributes)
            : base(propertyName, attributes)
        {
            ComponentType = componentType;
            PropertyType = propertyType;
        }

        public override Type ComponentType { get; }

        public override bool IsReadOnly => throw new NotImplementedException();

        public override Type PropertyType { get; }

        public override bool CanResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object component)
        {
            if (_getter == null)
            {
                _getter = CreateGetter(ComponentType, PropertyType, Name);
            }
            return _getter(component);
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            if (_setter == null)
            {
                _setter = CreateSetter(ComponentType, PropertyType, Name);
            }
            _setter(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            throw new NotImplementedException();
        }

        private Action<object, object> CreateSetter(Type componentType, Type valueType, string propertyName)
        {
            var component = Expression.Parameter(typeof(object));
            var value = Expression.Parameter(typeof(object));
            var castedComponent = Expression.Convert(component, componentType);
            var castedValue = Expression.ConvertChecked(value, valueType);
            var componentProperty = Expression.Property(castedComponent, propertyName);
            var valueAssignment = Expression.Assign(componentProperty, castedValue);
            var setterExpression = Expression.Lambda<Action<object, object>>(valueAssignment, component, value);
            return setterExpression.Compile();
        }

        private Func<object, object> CreateGetter(Type componentType, Type valueType, string propertyName)
        {
            var component = Expression.Parameter(typeof(object));
            var castedComponent = Expression.Convert(component, componentType);
            var componentProperty = Expression.Property(castedComponent, propertyName);
            var castedProperty = Expression.Convert(componentProperty, typeof(object));
            var getterExpression = Expression.Lambda<Func<object, object>>(castedProperty, component);
            return getterExpression.Compile();
        }
    }
}
