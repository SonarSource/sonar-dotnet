using System;
using System.Runtime.InteropServices;

SafeHandle handle = null;
IntPtr dangerousHandle = handle.DangerousGetHandle(); // Noncompliant {{Refactor the code to remove this use of 'SafeHandle.DangerousGetHandle'.}}

record R
{
    IntPtr Foo(SafeHandle handle) => handle.DangerousGetHandle(); // Noncompliant

    Func<SafeHandle, IntPtr> StaticLambda() => (SafeHandle h) => h.DangerousGetHandle(); // Noncompliant
}
