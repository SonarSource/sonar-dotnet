using System;

public class Foo
{
    public static void Method()
    {
        checked // Noncompliant, FN, nint/IntPtr is not considered an integral number type
        {
            nint x = 42;
            IntPtr y = 2;
            var _ = x + y;
        }

        checked // Noncompliant, FN, nuint/UIntPtr is not considered an integral number type
        {
            nuint x = 42;
            UIntPtr y = 2;
            var _ = x + y;
        }
    }
}
