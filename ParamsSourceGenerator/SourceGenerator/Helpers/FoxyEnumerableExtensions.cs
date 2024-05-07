using Microsoft.CodeAnalysis;
using System.Linq;

namespace Foxy.Params.SourceGenerator.Helpers
{
    internal static class FoxyEnumerableExtensions
    {
        public static IncrementalValuesProvider<TSource> NotNull<TSource>(
            this IncrementalValuesProvider<TSource?> enumerable)
            where TSource : class
        {
            return enumerable.Where(x => x is not null)!;
        }
    }
}
