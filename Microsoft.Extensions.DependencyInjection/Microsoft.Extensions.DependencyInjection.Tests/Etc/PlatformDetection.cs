// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Authentication;
using Microsoft.Win32;
using Xunit;

namespace System
{
    public static partial class PlatformDetection
    {
        //
        // Do not use the " { get; } = <expression> " pattern here. Having all the initialization happen in the type initializer
        // means that one exception anywhere means all tests using PlatformDetection fail. If you feel a value is worth latching,
        // do it in a way that failures don't cascade.
        //

        public static bool IsMonoRuntime => Type.GetType("Mono.RuntimeStructs") != null;
        public static bool IsNotMonoRuntime => !IsMonoRuntime;
        public static bool IsMonoAOT => Environment.GetEnvironmentVariable("MONO_AOT_MODE") == "aot";
        public static bool IsNativeAot => IsNotMonoRuntime && !IsReflectionEmitSupported;
        public static bool IsBrowser => RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
        public static bool IsWasi => RuntimeInformation.IsOSPlatform(OSPlatform.Create("WASI"));

        public static bool IsThreadingSupported => (!IsWasi && !IsBrowser) || IsWasmThreadingSupported;
        public static bool IsWasmThreadingSupported => IsBrowser && IsEnvironmentVariableTrue("IsBrowserThreadingSupported");

#if NETCOREAPP
        public static bool IsReflectionEmitSupported => RuntimeFeature.IsDynamicCodeSupported;
        public static bool IsNotReflectionEmitSupported => !IsReflectionEmitSupported;
#else
        public static bool IsReflectionEmitSupported => true;
#endif

        public static bool IsAssemblyLoadingSupported => !IsNativeAot;
        public static bool IsNonBundledAssemblyLoadingSupported => IsAssemblyLoadingSupported && !IsMonoAOT;

        private static bool IsEnvironmentVariableTrue(string variableName)
        {
            if (!IsBrowser)
                return false;

            return Environment.GetEnvironmentVariable(variableName) is "true";
        }
    }
}
