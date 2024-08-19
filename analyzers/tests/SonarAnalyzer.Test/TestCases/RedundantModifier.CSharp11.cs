using System;

public class Foo
{
    public static void Method()
    {
        checked // Noncompliant, FP, nint/IntPtr is not a member of KnownType.IntegralNumbers.
        {
            nint x = 42;
            IntPtr y = 2;
            var _ = x + y;
        }

        checked // Noncompliant, FP, nuint/UIntPtr is not a member of KnownType.IntegralNumbers.
        {
            nuint x = 42;
            UIntPtr y = 2;
            var _ = x + y;
        }
    }
}

// file-scoped types cannot use accessibility modifiers and cannot be nested.

file partial class PartialFoo { } // Noncompliant

file partial class PartialFooBar { } // Noncompliant

file partial class PartialFileClass { }
file partial class PartialFileClass { }


file unsafe class UnsafeClass
{
    int* pointer;
}

file unsafe class UnsafeClass2 // Noncompliant
//   ^^^^^^
{
    int num;
}

file unsafe interface MyInterface
{
    unsafe int* Method(); // Noncompliant
}
