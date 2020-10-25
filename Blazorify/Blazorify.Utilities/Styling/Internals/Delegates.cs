namespace Blazorify.Utilities.Styling.Internals
{
    public delegate void AddCssDelegate(string cssClass, bool condition);

    public delegate void ProcessCssDelegate(object cssContainer, AddCssDelegate addMethod);

    public delegate void AddStyleDelegate(string property, string value, bool condition);

    public delegate void ProcessStyleDelegate(object styleContainer, AddStyleDelegate addMethod);
}
