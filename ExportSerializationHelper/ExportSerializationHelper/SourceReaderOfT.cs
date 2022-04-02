using System;
using System.Linq.Expressions;

namespace ExportSerializationHelper
{
    public class SourceReader<TModel> : SourceReader where TModel : class
    {
        public void Map<TValue>(
            Expression<Func<TModel, TValue>> getter,
            Func<MemberConfigurator<TModel, TValue>, MemberConfigurator<TModel, TValue>>? configuratorFunc = null)
        {
            var type = typeof(TValue);
            var configurator = new MemberConfigurator<TModel, TValue>(new MemberConfiguration(type));
            if (configuratorFunc != null)
            {
                configurator = configuratorFunc(configurator);
            }
            configurator = configurator.Getter(getter.Compile());
            if (configurator.Configuration.IsNameUnset())
            {
                configurator.Name(ExtractName(getter));
            }
            AddMember(configurator.Configuration);
        }

        private string ExtractName(Expression getter)
        {
            NameExtractor extractor = new NameExtractor();
            extractor.Visit(getter);
            var name = string.Join(" ", extractor.ExtractedNames);
            return name ?? "";
        }
    }
}
