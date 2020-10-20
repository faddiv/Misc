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
}
