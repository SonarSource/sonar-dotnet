using System;

public class Foo
{
    public static void Method()
    {
        {
            nint x = 42;
            IntPtr y = 2;
            var _ = x + y;
        }

        {
            nuint x = 42;
            UIntPtr y = 2;
            var _ = x + y;
        }
    }
}

// file-scoped types cannot use accessibility modifiers and cannot be nested.

file class PartialFoo { } // Fixed

file class PartialFooBar { } // Fixed

file partial class PartialFileClass { }
file partial class PartialFileClass { }


file unsafe class UnsafeClass
{
    int* pointer;
}

file class UnsafeClass2 // Fixed
{
    int num;
}

file unsafe interface MyInterface
{
    int* Method(); // Fixed
}
