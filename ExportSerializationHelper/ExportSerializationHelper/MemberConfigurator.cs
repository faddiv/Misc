using System;

namespace ExportSerializationHelper
{
    public class MemberConfigurator<TModel, TValue>
        where TModel: class
    {
        public MemberConfigurator(MemberConfiguration configuration)
        {
            Configuration = configuration;
        }

        public MemberConfiguration Configuration { get; }

        public MemberConfigurator<TModel, TValue> Getter(Func<TModel, TValue> getter)
        {
            Configuration.Getter = (object value) => getter((TModel)value);
            return this;
        }

        public MemberConfigurator<TModel, TValue> Name(string name)
        {
            Configuration.Name = name;
            return this;
        }
    }
}
