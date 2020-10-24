namespace Blazorify.Utilities.Styling
{
    internal delegate void AddCssDelegate(string cssClass, bool condition);

    internal delegate void ProcessCssDelegate(object cssContainer, AddCssDelegate addMethod);

    internal delegate void AddStyleDelegate(string property, string value, bool condition);

    internal delegate void ProcessStyleDelegate(object styleContainer, AddStyleDelegate addMethod);
}
