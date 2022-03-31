using System;

namespace ExportSerializationHelper
{
    public abstract class SourceReader<TModel> : SourceReader where TModel : class
    {
        public void Map<TValue>(
            Func<TModel, TValue> getter,
            Func<MemberConfigurator<TModel, TValue>, MemberConfigurator<TModel, TValue>> configuratorFunc = null)
        {
            var configurator = new MemberConfigurator<TModel, TValue>(new MemberConfiguration(""));
            if (configuratorFunc != null)
            {
                configurator = configuratorFunc(configurator);
            }
            configurator = configurator.Getter(getter);
            AddMember(configurator.Configuration);
        }
    }
}
