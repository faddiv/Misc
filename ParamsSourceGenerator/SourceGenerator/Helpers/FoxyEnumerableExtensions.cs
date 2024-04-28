using System.Collections.Generic;
using System.Linq;

namespace Foxy.Params.SourceGenerator.Helpers
{
    internal static class FoxyEnumerableExtensions
    {
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Where(x => x != null);
        }
    }
}
