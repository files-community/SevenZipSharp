namespace SevenZip
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.Marshalling;

#if UNMANAGED
    internal static class ComInterop
    {
        private static readonly StrategyBasedComWrappers ComWrappers = new StrategyBasedComWrappers();

        public static unsafe T CreateInstance<T>(IntPtr module, Guid classId) where T : class
        {
            var createObjectAddress = NativeMethods.GetProcAddress(module, "CreateObject");
            if (createObjectAddress == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException("The 7-zip library does not export CreateObject.");
            }

            var interfaceId = typeof(T).GUID;
            var instance = IntPtr.Zero;
            var createObject = (delegate* unmanaged[Stdcall]<Guid*, Guid*, IntPtr*, int>)createObjectAddress;
            var result = createObject(&classId, &interfaceId, &instance);

            if (result < 0)
            {
                if (instance != IntPtr.Zero)
                {
                    Marshal.Release(instance);
                }

                Marshal.ThrowExceptionForHR(result);
            }

            if (instance == IntPtr.Zero)
            {
                throw new InvalidOperationException("The 7-zip library returned a null COM interface.");
            }

            try
            {
                return (T)ComWrappers.GetOrCreateObjectForComInstance(
                    instance,
                    CreateObjectFlags.UniqueInstance);
            }
            finally
            {
                Marshal.Release(instance);
            }
        }

        public static void Release(object instance)
        {
            if (instance is ComObject comObject)
            {
                comObject.FinalRelease();
            }
        }
    }
#endif
}
