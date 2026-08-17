[assembly: System.Runtime.CompilerServices.DisableRuntimeMarshalling]

namespace SevenZip
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.Marshalling;

#if UNMANAGED
    internal static partial class NativeMethods
    {
#if DESKTOP
        [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr LoadLibraryNative(string fileName);

        public static IntPtr LoadLibrary(string fileName) => LoadLibraryNative(fileName);
#else
        [LibraryImport("api-ms-win-core-libraryloader-l2-1-0.dll", EntryPoint = "LoadPackagedLibrary", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr LoadPackagedLibrary(string libraryName, int reserved);

        public static IntPtr LoadLibrary(string libraryName) => LoadPackagedLibrary(libraryName, 0);
#endif

#if DESKTOP
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool FreeLibrary(IntPtr hModule);
#else
        [LibraryImport("api-ms-win-core-libraryloader-l1-2-0.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool FreeLibrary(IntPtr hModule);
#endif

#if DESKTOP
        [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
        public static partial IntPtr GetProcAddress(IntPtr hModule, string procName);
#else
        [LibraryImport("api-ms-win-core-libraryloader-l1-2-0.dll", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
        public static partial IntPtr GetProcAddress(IntPtr hModule, string procName);
#endif

        public static T SafeCast<T>(PropVariant var, T def)
        {
            object obj;

            try
            {
                obj = var.Object;
            }
            catch (Exception)
            {
                return def;
            }

            if (obj is T expected)
            {
                return expected;
            }

            return def;
        }
    }
#endif
}
