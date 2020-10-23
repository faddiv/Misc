using System.Collections.Generic;

namespace Blazorify.Utilities.Styling
{
    internal struct StyleElement
    {
        public StyleElement(string property, string value)
        {
            Property = property;
            Value = value;
        }

        public string Property { get; }
        public string Value { get; }

        public override bool Equals(object obj)
        {
            return obj is StyleElement element &&
                   Property == element.Property &&
                   Value == element.Value;
        }

        public override int GetHashCode()
        {
            int hashCode = -1027930222;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Property);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        public static bool operator ==(StyleElement left, StyleElement right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StyleElement left, StyleElement right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{Property}:{Value}";
        }
    }
}
