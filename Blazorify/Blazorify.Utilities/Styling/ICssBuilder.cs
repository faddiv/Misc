using System;

namespace Blazorify.Utilities.Styling
{
    public interface ICssBuilder
    {
        CssDefinition Create(CssBuilderOptions options = null);

        /// <summary>
        /// Creates an instace from the CssBuilder and adds all the arguments as class. The result is produced by .ToString().
        /// </summary>
        /// <param name="arguments">List of values that cen be converted to css classes. null and empty strings skipped.</param>
        /// <returns>A CssDefinition instance that contains the processed arguments, and can be used in the class attribute directly.</returns>
        /// <example>
        ///     &lt;div class="@Css["class1", ("class2", true), new { class3 = true}]&gt;...&lt;/div&gt;
        /// </example>
        CssDefinition this[params object[] arguments] { get; }

        /// <summary>
        /// Creates an instace from the CssBuilder and adds all the arguments as class. The result is produced by .ToString().
        /// </summary>
        /// <param name="cssClasses">Default classes to add. If it is null or empty then skipped.</param>
        /// <param name="tuple">List of tuple values that evaulated and added conditionally.</param>
        /// <returns>A CssDefinition instance that contains the processed arguments, and can be used in the class attribute directly.</returns>
        /// <remarks>
        /// This variant created so Func&lt;bool&gt; also can be used on this call.
        /// </remarks>
        CssDefinition this[string cssClasses, params (string, Func<bool>)[] tuple] { get; }
    }
}
