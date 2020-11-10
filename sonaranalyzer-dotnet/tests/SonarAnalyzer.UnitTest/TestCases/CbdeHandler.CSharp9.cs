using System;
using System.Collections.Generic;
using System.Linq;

int i = 2147483600;
i += 100; // FN, Top level statements are not supported

public class NativeInt
{
    public void PositiveOverflowNativeInt()
    {
        nint i = 2147483600;
        i += 100; // Compliant, we can't tell what's the native MaxValue on the run machine

        nuint ui = (nuint)18446744073709551615;
        ui += 100; // Compliant, we can't tell what's the native MaxValue on the run machine
    }

    public void NegativeOverflowNativeInt()
    {
        nint i = -2147483600;
        i -= 100;  // Compliant, we can't tell what's the native MinValue for the run machine
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            int i = -2147483600;
            i -= 100; // FN, lambdas are not supported
        };
    }

    public void LambdaDiscard(IEnumerable<int> items)
    {
        var result = items.Select((_, _) =>
        {
            int i = -2147483600;
            i -= 100; // FN, lambdas are not supported
            return i;
        });
    }
}

public class Properties
{
    public int GetInit
    {
        get
        {
            int i = 2147483600;
            i += 100; // FN, properties are not supported
            return i;
        }
        init
        {
            int i = 2147483600;
            i += 100; // FN, properties are not supported
        }
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
        TargetTypedNew s = new(); // This was throwing CbdeException: Top level error in CBDE handling
    }
}

public unsafe class FunctionPointers
{
    public static void Log() { }

    public void Method()
    {
        delegate*<void> ptr1 = &Log;
        delegate*<void> ptr2 = &FunctionPointers.Log;
    }
}
