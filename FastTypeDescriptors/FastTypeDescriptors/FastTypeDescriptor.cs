using System;
using System.ComponentModel;
using System.Reflection;

namespace FastTypeDescriptors
{

    public class FastTypeDescriptor : CustomTypeDescriptor
    {
        private readonly Type type;

        public FastTypeDescriptor(ICustomTypeDescriptor parent, Type type) : base(parent)
        {
            this.type = type;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var properties = new PropertyDescriptorCollection(null);
            foreach (var info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var objs = info.GetCustomAttributes(true);
                var attrs = new Attribute[objs.Length];
                Array.Copy(objs, attrs, attrs.Length);
                var desc = new FastPropertyDescriptor(type, info.PropertyType, info.Name, attrs);
                properties.Add(desc);
            }
            return properties;
        }
    }
}
