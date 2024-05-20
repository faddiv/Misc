namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Specifies that an output is not null even if the corresponding type allows it. Specifies that an input argument was not null when the call returns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false)]
    public sealed class NotNullAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the System.Diagnostics.CodeAnalysis.NotNullAttribute class.
        /// </summary>
        public NotNullAttribute() { }
    }
}
