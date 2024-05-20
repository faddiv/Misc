namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Indicates that compiler support for a particular feature is required for the location where this attribute is applied.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        /// <summary>
        /// The System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute.FeatureName used for the ref structs C# feature.
        /// </summary>
        public const string RefStructs = "RefStructs";

        /// <summary>
        /// The System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute.FeatureName used for the required members C# feature.
        /// </summary>
        public const string RequiredMembers = "RequiredMembers";

        /// <summary>
        /// Initializes a System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute instance for the passed in compiler feature.
        /// </summary>
        /// <param name="featureName">The name of the required compiler feature.</param>
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        //
        // Summary:
        //     The name of the compiler feature.
        public string FeatureName { get; }

        /// <summary>
        /// Gets a value that indicates whether the compiler can choose to allow access to the location where this attribute is applied if it does not understand System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute.FeatureName.
        /// </summary>
        public bool IsOptional { get; init; }
    }
}
