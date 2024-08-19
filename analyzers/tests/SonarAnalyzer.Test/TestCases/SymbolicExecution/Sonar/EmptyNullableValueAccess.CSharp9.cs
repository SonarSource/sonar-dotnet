using System;
using System.Diagnostics.CodeAnalysis;

int? topLevel = 42;
var v = topLevel.Value;

topLevel = null;
v = topLevel.Value;   // FN

void TopLevelLocalFunction()
{
    int? local = 42;
    var v = local.Value;

    local = null;
    v = local.Value;   // Noncompliant
}

public class Sample
{
    private int field;

    public void TargetTypedNew()
    {
        int? nullable;

        nullable = new();
        var v = nullable.Value;

        nullable = null;
        v = nullable.Value; // FN, can't build CFG for this method
    }

    public void PatternMatching(int? arg)
    {
        int v;
        if (arg is int)
        {
            v = arg.Value;     // Compliant
        }

        if (arg is int and > 0 and > 1)
        {
            v = arg.Value;     // Compliant
        }

        if (arg is null)
        {
            v = arg.Value;     // FN
        }
        if (arg is int or null)
        {
            v = arg.Value;     // FN
        }
        else if (arg is not not null)
        {
            v = arg.Value;     // FN
        }
        else if (!(arg is not null))
        {
            v = arg.Value;     // FN
        }
        else
        {
            int? nullable = null;
            if (nullable is not null)
            {
                v = nullable.Value;
            }
            v = nullable.Value;        // FN, can't build CFG for this method
        }
    }

    public void StaticLambda()
    {
        Func<int> a = static () =>
        {
            int? nullable = null;
            return nullable.Value;    // Noncompliant
        };
        a();
    }

    public int PropertySimple
    {
        get => 42;
        init
        {
            int? nullable = null;
            field = nullable.Value;    // Noncompliant
        }
    }

    public int? PropertyWithValue
    {
        get => null;
        init
        {
            if (value == null)
            {
                field = value.Value;   // FN
            }
        }
    }
}

public record Record
{
    public void Method()
    {
        int? nullable = null;
        var v = nullable.Value;    // Noncompliant
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
        int? nullable = null;
        var v = nullable.Value;    // Noncompliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6682
public struct Repro_6682
{
    public bool SomeProperty { get; }

    public void PatternMatching(Repro_6682? arg, bool condition)
    {
        if (condition)
        {
            arg = null;
        }

        if (arg is { SomeProperty: true })  // A null check
        {
            var value = arg.Value;  // Noncompliant FP
        }

        if (arg is { })             // A null check
        {
            var value = arg.Value;  // Noncompliant FP
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8633
public class Repro_8633
{
    public TimeOnly ThrowHelper()
    {
        var time = LoadCurrentTimeFromProvider();
        if (!time.HasValue)
        {
            ExceptionHelper.ThrowBecauseOfMissingTime();
        }

        return time.Value; // Noncompliant FP
    }

    private TimeOnly? LoadCurrentTimeFromProvider() => null;

    public static class ExceptionHelper
    {
        [DoesNotReturn]
        public static void ThrowBecauseOfMissingTime() => throw new ArgumentNullException();
    }
}
