using System;

namespace Blazorify.Utilities.Styling
{
    public interface IStyleBuilder
    {
        StyleDefinition Create();

        /// <summary>
        /// Creates an instace from the StyleBuilder and calls the <see cref="StyleDefinition.AddMultiple(object[])"/> with the given arguments.
        /// </summary>
        /// <param name="arguments">List of values that cen be converted to styles.</param>
        /// <returns>A StyleBuilder instance that contains the processed arguments, and can be used in the style attribute directly.</returns>
        /// <example>
        ///     &lt;div style="@Styles[("width", "100px"),("height", "200px", true)]"&gt;...&lt;/div&gt;
        /// </example>
        StyleDefinition this[params object[] values] { get; }

        StyleDefinition this[params (string, string, Func<bool>)[] values] { get; }

        StyleDefinition this[params (string, Func<string>, bool)[] values] { get; }

        StyleDefinition this[params (string, Func<string>, Func<bool>)[] values] { get; }
    }
}
