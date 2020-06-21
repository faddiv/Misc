using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using Xunit;

namespace FastTypeDescriptors.SpeedTests
{
    public class Class1Tests
    {
        private const int runs = 1200000;

        [Fact]
        public void Direct()
        {
            var class1 = new Direct();
            var poco = new Poco();
            var sv = Stopwatch.StartNew();
            for (int i = 0; i < runs; i++)
            {
                var g = Guid.NewGuid();
                class1.SetId(poco, g);
                class1.SetValue(poco, g.ToString());
            }
            sv.Stop();
            Assert.True(false, $"Elapsed: {sv.Elapsed}");
        }
        [Fact]
        public void Reflection()
        {
            var class1 = new Reflection();
            var poco = new Poco();
            var sv = Stopwatch.StartNew();
            for (int i = 0; i < runs; i++)
            {
                var g = Guid.NewGuid();
                class1.SetId(poco, g);
                class1.SetValue(poco, g.ToString());
            }
            sv.Stop();
            Assert.True(false, $"Elapsed: {sv.Elapsed}");
        }

        [Fact]
        public void LambdaAccess()
        {
            var class1 = new LambdaAccess();
            var poco = new Poco();
            var sv = Stopwatch.StartNew();
            for (int i = 0; i < runs; i++)
            {
                var g = Guid.NewGuid();
                class1.SetId(poco, g);
                class1.SetValue(poco, g.ToString());
            }
            sv.Stop();
            Assert.True(false, $"Elapsed: {sv.Elapsed}");
        }

        [Fact]
        public void LocalizedName()
        {
            var cl = new Reflection();
            Assert.Equal("Azonosító", cl.IdName);
            Assert.Equal("Érték", cl.ValueName);
        }
    }
    public class Reflection
    {
        private PropertyDescriptorCollection _props;
        private PropertyDescriptor _id;
        private PropertyDescriptor _value;

        public Reflection()
        {
            _props = TypeDescriptor.GetProperties(typeof(Poco));
            _id = _props[nameof(Poco.Id)];
            _value = _props[nameof(Poco.Value)];
            _id.SetValue(new Poco(), Guid.NewGuid());
            _value.SetValue(new Poco(), "");
        }
        public void SetId(Poco p, Guid value)
        {
            _id.SetValue(p, value);
        }
        public void SetValue(Poco p, string value)
        {
            _value.SetValue(p, value);
        }
        public string IdName => _id.DisplayName;
        public string ValueName => _value.DisplayName;
    }

    public class Direct
    {
        public Direct()
        {
        }
        public void SetId(Poco p, Guid value)
        {
            p.Id = value;
        }
        public void SetValue(Poco p, string value)
        {
            p.Value = value;
        }

        public Guid GetId(Poco p)
        {
            return p.Id;
        }
        public string GetValue(Poco p)
        {
            return p.Value;
        }
        public string IdName => "Id";
        public string ValueName => "Value";
    }

    public class LambdaAccess
    {
        private Action<object, object> idSetter;
        private Action<object, object> valueSetter;
        public LambdaAccess()
        {
            idSetter = CreateSetter(typeof(Poco), typeof(Guid), "Id");
            valueSetter = CreateSetter(typeof(Poco), typeof(string), "Value");
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
        public void SetId(Poco p, Guid value)
        {
            idSetter(p, value);
        }
        public void SetValue(Poco p, string value)
        {
            valueSetter(p, value);
        }

        public Guid GetId(Poco p)
        {
            return p.Id;
        }
        public string GetValue(Poco p)
        {
            return p.Value;
        }
        public string IdName => "Id";
        public string ValueName => "Value";
    }
    public class Poco
    {
        [System.ComponentModel.DataAnnotations.Display(ResourceType = typeof(Resource1), Name = "Id")]
        public Guid Id { get; set; }
        [System.ComponentModel.DataAnnotations.Display(ResourceType = typeof(Resource1), Name = "Value")]
        public string Value { get; set; }
    }
}
