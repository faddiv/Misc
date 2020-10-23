using System;
using System.Reflection;
using System.Text;

namespace Blazorify.Utilities.Styling
{
    public class CssBuilderNamingConventions
    {

        private const char Hyphen = '-';
        private const char Underscore = '_';

        public static string None(PropertyInfo info)
        {
            return info.Name;
        }

        public static string None(Enum value)
        {
            return value.ToString();
        }

        public static string UnderscoreToHyphen(PropertyInfo info)
        {
            return info.Name.Replace(Underscore, Hyphen);
        }

        public static string UnderscoreToHyphen(Enum value)
        {
            return value.ToString().Replace(Underscore, Hyphen);
        }

        public static string KebabCase(PropertyInfo info)
        {
            return KebabCase(info.Name, false);
        }

        public static string KebabCase(Enum value)
        {
            return KebabCase(value.ToString(), false);
        }

        public static string KebabCaseWithUnderscoreToHyphen(PropertyInfo info)
        {
            return KebabCase(info.Name, true);
        }

        public static string KebabCaseWithUnderscoreToHyphen(Enum value)
        {
            return KebabCase(value.ToString(), true);
        }

        private static string KebabCase(string name, bool underscoreToHyphen)
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
