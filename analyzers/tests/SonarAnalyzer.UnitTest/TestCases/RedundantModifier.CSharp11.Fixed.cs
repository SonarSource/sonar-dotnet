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
