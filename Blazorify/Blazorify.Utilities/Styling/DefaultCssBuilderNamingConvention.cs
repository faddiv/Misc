using System;
using System.Reflection;
using System.Text;

namespace Blazorify.Utilities.Styling
{
    public class DefaultCssBuilderNamingConvention : ICssBuilderNamingConvention
    {
        private const char Hyphen = '-';
        private const char Underscore = '_';

        public bool PropertyUnderscoreToHyphen { get; set; } = true;

        public CssBuilderNamingMode PropertyMode { get; set; }
            = CssBuilderNamingMode.KebabCase;

        public bool EnumUnderscoreToHyphen { get; set; } = true;

        public CssBuilderNamingMode EnumMode { get; set; }
            = CssBuilderNamingMode.KebabCase;

        public string ToCssClassName(PropertyInfo property)
        {
            string name = property.Name;
            switch (PropertyMode)
            {
                case CssBuilderNamingMode.KebabCase:
                    return KebabCase(name, PropertyUnderscoreToHyphen);
                default:
                    return UnderscoreToHyphen(name, PropertyUnderscoreToHyphen);
            }
        }

        public string ToCssClassName(Enum enumValue)
        {
            string name = enumValue.ToString();
            switch (EnumMode)
            {
                case CssBuilderNamingMode.KebabCase:
                    return KebabCase(name, EnumUnderscoreToHyphen);
                default:
                    return UnderscoreToHyphen(name, EnumUnderscoreToHyphen);
            }
        }

        private string UnderscoreToHyphen(string name, bool underscoreToHyphen)
        {
            if (underscoreToHyphen)
            {
                return name.Replace('_', '-');
            }
            return name;

        }

        private string KebabCase(string name, bool underscoreToHyphen)
        {
            var builder = new StringBuilder(name.Length * 2);
            builder.Append(char.ToLowerInvariant(name[0]));
            for (int i = 1; i < name.Length; i++)
            {
                var ch = name[i];
                if (underscoreToHyphen && ch == Underscore)
                {
                    builder.Append(Hyphen);
                }
                else if (char.IsUpper(ch))
                {
                    builder.Append(Hyphen);
                    builder.Append(char.ToLower(ch));
                }
                else
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }
    }
}
