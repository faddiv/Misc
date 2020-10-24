using System;

namespace Blazorify.Utilities.Styling
{
    public interface ICssBuilder
    {
        CssDefinition Create(CssBuilderOptions options = null);

        /// <summary>
        /// Creates an instace from the CssBuilder and calls the <see cref="CssDefinition.AddMultiple(object[])"/> with the given arguments.
        /// </summary>
        /// <param name="arguments">List of values that cen be converted to css classes.</param>
        /// <returns>A CssBuilder instance that contains the processed arguments, and can be used in the class attribute directly.</returns>
        /// <example>
        ///     &lt;div class="@Css["class1", ("class2", true), new { class3 = true}]&gt;...&lt;/div&gt;
        /// </example>
        CssDefinition this[params object[] values] { get; }

        CssDefinition this[string cssClasses, params (string, Func<bool>)[] tuple] { get; }
    }
}
