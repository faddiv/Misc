using System;
using System.Collections;
using System.ComponentModel;

namespace FastTypeDescriptors
{
    public class FastTypeDescriptorProvider : TypeDescriptionProvider
    {
        private static TypeDescriptionProvider parent;
        public static FastTypeDescriptorProvider Instance { get; } = new FastTypeDescriptorProvider();
        public static void Register(Type type)
        {
            parent = TypeDescriptor.GetProvider(type);
            TypeDescriptor.AddProvider(Instance, type);
        }
        public FastTypeDescriptorProvider()
        {
        }

        public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            var result = parent.CreateInstance(provider, objectType, argTypes, args);
            return result;
        }
        public override IDictionary GetCache(object instance)
        {
            var result = parent.GetCache(instance);
            return result;
        }
        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            var result = parent.GetExtendedTypeDescriptor(instance);
            return result;
        }
        protected override IExtenderProvider[] GetExtenderProviders(object instance)
        {
            var result = base.GetExtenderProviders(instance);
            return result;
        }
        public override string GetFullComponentName(object component)
        {
            var result = parent.GetFullComponentName(component);
            return result;
        }
        public override Type GetReflectionType(Type objectType, object instance)
        {
            var result = parent.GetReflectionType(objectType, instance);
            return result;
        }
        public override Type GetRuntimeType(Type reflectionType)
        {
            var result = parent.GetRuntimeType(reflectionType);
            return result;
        }
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            var result = parent.GetTypeDescriptor(objectType, instance);
            return new FastTypeDescriptor(result, objectType);
        }
        public override bool IsSupportedType(Type type)
        {
            var result = parent.IsSupportedType(type);
            return result;
        }
    }
}
