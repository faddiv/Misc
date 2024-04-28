using System;
using System.Collections.Generic;

namespace Foxy.Params.SourceGenerator.Data
{
    internal class TypeCandidate : IEquatable<TypeCandidate>
    {
        public string Namespace { get; internal set; }
        public string TypeName { get; internal set; }
        public string CreateFileName()
        {
            return Namespace == ""
                ? $"{TypeName}.g.cs"
                : $"{Namespace}.{TypeName}.g.cs";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TypeCandidate);
        }

        public bool Equals(TypeCandidate other)
        {
            return !(other is null) &&
                   Namespace == other.Namespace &&
                   TypeName == other.TypeName;
        }

        public override int GetHashCode()
        {
            int hashCode = -96582770;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Namespace);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
            return hashCode;
        }

        public static bool operator ==(TypeCandidate left, TypeCandidate right)
        {
            return EqualityComparer<TypeCandidate>.Default.Equals(left, right);
        }

        public static bool operator !=(TypeCandidate left, TypeCandidate right)
        {
            return !(left == right);
        }
    }
}

