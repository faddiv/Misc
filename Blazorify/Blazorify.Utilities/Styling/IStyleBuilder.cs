using System;

namespace Blazorify.Utilities.Styling
{
    public interface IStyleBuilder
    {
        /// <summary>
        /// Starts an empty StyleDefinition. The result is produced by .ToString().
        /// </summary>
        /// <returns>An empty StyleDefinition.</returns>
        StyleDefinition Create();

        /// <summary>
        /// Starts a StyleDefinition and adds all the arguments as stlye. The result is produced by .ToString().
        /// </summary>
        /// <param name="arguments">List of values that cen be converted to styles.</param>
        /// <returns>A StyleDefinition instance that contains the processed arguments, and can be used in the style attribute directly.</returns>
        /// <example>
        ///     &lt;div style="@Styles[("width", "100px"),("height", "200px", true)]"&gt;...&lt;/div&gt;
        /// </example>
        StyleDefinition this[params object[] arguments] { get; }

        /// <summary>
        /// Starts a StyleDefinition and adds all the arguments as stlye. The result is produced by .ToString().
        /// </summary>
        /// <param name="arguments">List of values that cen be converted to styles.</param>
        /// <returns>A StyleDefinition instance that contains the processed arguments, and can be used in the style attribute directly.</returns>
        StyleDefinition this[params (string, string, Func<bool>)[] arguments] { get; }

        /// <summary>
        /// Starts a StyleDefinition and adds all the arguments as stlye. The result is produced by .ToString().
        /// </summary>
        /// <param name="arguments">List of values that cen be converted to styles.</param>
        /// <returns>A StyleDefinition instance that contains the processed arguments, and can be used in the style attribute directly.</returns>
        StyleDefinition this[params (string, Func<string>, bool)[] arguments] { get; }

        /// <summary>
        /// Starts a StyleDefinition and adds all the arguments as stlye. The result is produced by .ToString().
        /// </summary>
        /// <param name="arguments">List of values that cen be converted to styles.</param>
        /// <returns>A StyleDefinition instance that contains the processed arguments, and can be used in the style attribute directly.</returns>
        StyleDefinition this[params (string, Func<string>, Func<bool>)[] arguments] { get; }
    }
}
