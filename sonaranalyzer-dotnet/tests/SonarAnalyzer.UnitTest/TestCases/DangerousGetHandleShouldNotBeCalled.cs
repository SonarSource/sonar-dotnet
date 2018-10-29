using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Tests.Diagnostics
{
    class Program
    {
        private static IntPtr GetRegistryKeyHandle(RegistryKey rKey)
        {
            Type registryKeyType = typeof(RegistryKey);

            System.Reflection.FieldInfo fieldInfo =
                registryKeyType.GetField("hkey", System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (fieldInfo != null)
            {
                SafeHandle handle = (SafeHandle)fieldInfo.GetValue(rKey);
                IntPtr dangerousHandle = handle.DangerousGetHandle(); // Noncompliant {{Refactor the code to remove this use of 'SafeHandle.DangerousGetHandle'.}}
//                                              ^^^^^^^^^^^^^^^^^^
                IntPtr dangerousHandle2 = (((handle))).DangerousGetHandle(); // Noncompliant
                IntPtr? dangerousHandle3 = handle?.DangerousGetHandle(); // Noncompliant
                DangerousGetHandle();
                return dangerousHandle;
            }

            return IntPtr.Zero;
        }

        private static IntPtr DangerousGetHandle()
        {
            return IntPtr.Zero;
        }
    }
}
