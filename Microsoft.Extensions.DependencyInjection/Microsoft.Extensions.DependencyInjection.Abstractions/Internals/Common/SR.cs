// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Resources;

namespace System
{
    internal static partial class SR
    {
        private static readonly bool s_usingResourceKeys =
            AppContext.TryGetSwitch("System.Resources.UseSystemResourceKeys", out bool usingResourceKeys)
                ? usingResourceKeys
                : false;

        public static string MultipleCtorsFoundWithBestLength => GetResourceString("MultipleCtorsFoundWithBestLength");
        public static string CannotCreateAbstractClasses => GetResourceString("CannotCreateAbstractClasses");
        public static string UnableToResolveService => GetResourceString("UnableToResolveService");
        public static string CtorNotLocated => GetResourceString("CtorNotLocated");
        public static string MultipleCtorsFound => GetResourceString("MultipleCtorsFound");
        public static string KeyedServicesNotSupported => GetResourceString("KeyedServicesNotSupported");
        public static string MultipleCtorsMarkedWithAttribute => GetResourceString("MultipleCtorsMarkedWithAttribute");
        public static string MarkedCtorMissingArgumentTypes => GetResourceString("MarkedCtorMissingArgumentTypes");
        public static string ServiceCollectionReadOnly => GetResourceString("ServiceCollectionReadOnly");
        public static string NonKeyedDescriptorMisuse => GetResourceString("NonKeyedDescriptorMisuse");
        public static string NoServiceRegistered => GetResourceString("NoServiceRegistered");

        public static string TryAddIndistinguishableTypeToEnumerable =>
            GetResourceString("TryAddIndistinguishableTypeToEnumerable");

        public static string AsyncDisposableServiceDispose => GetResourceString("AsyncDisposableServiceDispose");

        public static string ConstantCantBeConvertedToServiceType =>
            GetResourceString("ConstantCantBeConvertedToServiceType");

        public static string ImplementationTypeCantBeConvertedToServiceType =>
            GetResourceString("ImplementationTypeCantBeConvertedToServiceType");

        public static string CallSiteTypeNotSupported => GetResourceString("CallSiteTypeNotSupported");

        public static string DirectScopedResolvedFromRootException =>
            GetResourceString("DirectScopedResolvedFromRootException");

        public static string ScopedResolvedFromRootException => GetResourceString("ScopedResolvedFromRootException");
        public static string ScopedInSingletonException => GetResourceString("ScopedInSingletonException");
        public static string CircularDependencyException => GetResourceString("CircularDependencyException");
        public static string OpenGenericServiceRequiresOpenGenericImplementation => GetResourceString("OpenGenericServiceRequiresOpenGenericImplementation");
        public static string TypeCannotBeActivated => GetResourceString("TypeCannotBeActivated");
        public static string ArityOfOpenGenericServiceNotEqualArityOfOpenGenericImplementation => GetResourceString("ArityOfOpenGenericServiceNotEqualArityOfOpenGenericImplementation");
        public static string TrimmingAnnotationsDoNotMatch => GetResourceString("TrimmingAnnotationsDoNotMatch");
        public static string TrimmingAnnotationsDoNotMatch_NewConstraint => GetResourceString("TrimmingAnnotationsDoNotMatch_NewConstraint");
        public static string AotCannotCreateEnumerableValueType => GetResourceString("AotCannotCreateEnumerableValueType");
        public static string InvalidServiceDescriptor => GetResourceString("InvalidServiceDescriptor");
        public static string NoConstructorMatch => GetResourceString("NoConstructorMatch");
        public static string AmbiguousConstructorException => GetResourceString("AmbiguousConstructorException");
        public static string UnableToActivateTypeException => GetResourceString("UnableToActivateTypeException");
        public static string CannotResolveService => GetResourceString("CannotResolveService");
        public static string AotCannotCreateGenericValueType => GetResourceString("AotCannotCreateGenericValueType");
        public static string InvalidServiceKeyType => GetResourceString("InvalidServiceKeyType");
        public static string ServiceDescriptorNotExist => GetResourceString("ServiceDescriptorNotExist");
        public static string GetCaptureDisposableNotSupported => GetResourceString("GetCaptureDisposableNotSupported");

        // This method is used to decide if we need to append the exception message parameters to the message when calling SR.Format.
        // by default it returns the value of System.Resources.UseSystemResourceKeys AppContext switch or false if not specified.
        // Native code generators can replace the value this returns based on user input at the time of native code generation.
        // The trimming tools are also capable of replacing the value of this method when the application is being trimmed.
        internal static bool UsingResourceKeys() => s_usingResourceKeys;

        // We can optimize out the resource string blob if we can see all accesses to it happening
        // through the generated SR.XXX properties.
        // If a call to GetResourceString is left, the optimization gets defeated and we need to keep
        // the whole resource blob. It's important to keep this private. CoreCLR's CoreLib gets a free
        // pass because the VM needs to be able to call into this, but that's a known set of constants.
#if CORECLR || LEGACY_GETRESOURCESTRING_USER
        internal
#else
        private
#endif
            static string GetResourceString(string resourceKey)
        {
            if (UsingResourceKeys())
            {
                return resourceKey;
            }

            string? resourceString = null;
            try
            {
                resourceString =
#if SYSTEM_PRIVATE_CORELIB || NATIVEAOT
                    InternalGetResourceString(resourceKey);
#else
                    resourceKey;
#endif
            }
            catch (MissingManifestResourceException)
            {
            }

            return resourceString!; // only null if missing resources
        }

#if LEGACY_GETRESOURCESTRING_USER
        internal
#else
        private
#endif
            static string GetResourceString(string resourceKey, string defaultString)
        {
            string resourceString = GetResourceString(resourceKey);

            return resourceKey == resourceString || resourceString == null ? defaultString : resourceString;
        }

        internal static string Format(string resourceFormat, object? p1)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1);
            }

            return string.Format(resourceFormat, p1);
        }

        internal static string Format(string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2);
            }

            return string.Format(resourceFormat, p1, p2);
        }

        internal static string Format(string resourceFormat, object? p1, object? p2, object? p3)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2, p3);
            }

            return string.Format(resourceFormat, p1, p2, p3);
        }

        internal static string Format(string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys())
                {
                    return resourceFormat + ", " + string.Join(", ", args);
                }

                return string.Format(resourceFormat, args);
            }

            return resourceFormat;
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1);
            }

            return string.Format(provider, resourceFormat, p1);
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2);
            }

            return string.Format(provider, resourceFormat, p1, p2);
        }

        internal static string Format(
            IFormatProvider? provider,
            string resourceFormat,
            object? p1,
            object? p2,
            object? p3)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2, p3);
            }

            return string.Format(provider, resourceFormat, p1, p2, p3);
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys())
                {
                    return resourceFormat + ", " + string.Join(", ", args);
                }

                return string.Format(provider, resourceFormat, args);
            }

            return resourceFormat;
        }
    }
}
