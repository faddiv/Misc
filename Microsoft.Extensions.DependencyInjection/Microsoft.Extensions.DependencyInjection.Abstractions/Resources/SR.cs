// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Resources;

namespace Microsoft.Extensions.DependencyInjection.Abstractions.Resources
{
    internal static partial class SR
    {
        private static readonly bool s_usingResourceKeys =
            AppContext.TryGetSwitch("System.Resources.UseSystemResourceKeys", out bool usingResourceKeys)
                ? usingResourceKeys
                : false;

        public static string MultipleCtorsFoundWithBestLength => Strings.MultipleCtorsFoundWithBestLength;
        public static string CannotCreateAbstractClasses => Strings.CannotCreateAbstractClasses;
        public static string UnableToResolveService => Strings.UnableToResolveService;
        public static string CtorNotLocated => Strings.CtorNotLocated;
        public static string MultipleCtorsFound => Strings.MultipleCtorsFound;
        public static string KeyedServicesNotSupported => Strings.KeyedServicesNotSupported;
        public static string MultipleCtorsMarkedWithAttribute => Strings.MultipleCtorsMarkedWithAttribute;
        public static string MarkedCtorMissingArgumentTypes => Strings.MarkedCtorMissingArgumentTypes;
        public static string ServiceCollectionReadOnly => Strings.ServiceCollectionReadOnly;
        public static string NonKeyedDescriptorMisuse => Strings.NonKeyedDescriptorMisuse;
        public static string NoServiceRegistered => Strings.NoServiceRegistered;

        public static string TryAddIndistinguishableTypeToEnumerable =>
            Strings.TryAddIndistinguishableTypeToEnumerable;

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
