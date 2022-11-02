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
