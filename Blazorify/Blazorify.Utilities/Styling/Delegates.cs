namespace Blazorify.Utilities.Styling
{
    /// <summary>
    /// Delegate for the CssBuilder add method.
    /// </summary>
    /// <param name="cssClass">a css class to add the builder.</param>
    /// <param name="condition">If true then the css value added to the builder.</param>
    public delegate void AddDelegate(string cssClass, bool condition);

    /// <summary>
    /// Delegate for the method which adds the css-es from the object to the CssBuilder.
    /// </summary>
    /// <param name="cssContainer">An object instance (usually anonymous) containing the css values as properties.</param>
    /// <param name="addMethod">The add method of the CssBuilder.</param>
    public delegate void ProcessObjectDelegate(object cssContainer, AddDelegate addMethod);

    /// <summary>
    /// Creates an instace from the CssBuilder and calls the <see cref="CssBuilder.AddMultiple(object[])"/> with the given arguments.
    /// </summary>
    /// <param name="arguments">List of values that cen be converted to css classes.</param>
    /// <returns>A CssBuilder instance that contains the processed arguments, and can be used in the class attribute directly.</returns>
    /// <example>
    ///     &lt;div class="@Css("class1", ("class2", true), new { class3 = true})&gt;...&lt;/div&gt;
    /// </example>
    public delegate ICssBuilder CssBuilderDelegate(params object[] arguments);


    /// <summary>
    /// Creates an instace from the StyleBuilder and calls the <see cref="StyleBuilder.AddMultiple(object[])"/> with the given arguments.
    /// </summary>
    /// <param name="arguments">List of values that cen be converted to styles.</param>
    /// <returns>A StyleBuilder instance that contains the processed arguments, and can be used in the style attribute directly.</returns>
    /// <example>
    ///     &lt;div style="@Styles(("width", "100px"),("height", "200px", true))"&gt;...&lt;/div&gt;
    /// </example>
    public delegate IStyleBuilder StyleBuilderDelegate(params object[] arguments);
}
