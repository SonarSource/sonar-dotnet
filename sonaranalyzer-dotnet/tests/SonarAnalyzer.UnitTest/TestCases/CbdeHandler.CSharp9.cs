using System;

namespace Tests.Diagnostics
{
    public class Sample
    {
        public void PositiveOverflowNativeInt()
        {
            nint i = 2147483600;
            i += 100; // FN, C# 9 native int is not supported yet
        }
        public void NegativeOverflowNativeInt()
        {
            nint i = -2147483600;
            i -= 100; // FN, C# 9 native int is not supported yet
        }
    }

    public record SampleRecord
    {
        public void Inc()
        {
            int i = 2147483600;
            i += 100; // Noncompliant
        }
    }

    public partial class Partial
    {
        public partial void Method();
    }

    public partial class Partial
    {
        public partial void Method()
        {
            int i = 2147483600;
            i += 100; // Noncompliant
        }
    }

    public class TargetTypedNew
    {
        public void Go()
        {
            Sample s = new(); // This was throwing CbdeException: Top level error in CBDE handling
        }
    }
}
