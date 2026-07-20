using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

#nullable enable

#region S8969 - redundant because flow analysis already proves non-null (warnings enabled)

public class AlreadyNotNull
{
    public void Method()
    {
        object a = new object();
        _ = a; // Fixed
    }
}

public class TrulyMaybeNull
{
    public void Method()
    {
        object? a = null;
        _ = a!; // Compliant, "a" is genuinely null here, the suppression is needed
    }
}

public class NarrowedByNullCheck
{
    public void Method(string? a)
    {
        if (a != null)
        {
            _ = a; // Fixed
        }
    }
}

public class NarrowedByLoopExitCondition
{
    public void Method(string? a)
    {
        while (a == null)
        {
        }
        _ = a; // Fixed
    }
}

public class ConditionallyReassignedInsideLoop
{
    public void Method(string? a, bool condition)
    {
        for (var i = 0; i < 10; i++)
        {
            if (condition)
            {
                a = null;
            }
            _ = a!; // Compliant, "a" can be reassigned to null on a previous iteration
        }
    }
}

public class ReassignedInTry
{
    public void Method(string? a, bool condition)
    {
        try
        {
            a = condition ? "x" : null;
        }
        finally
        {
            _ = a!; // Compliant, the try body may throw before completing the reassignment
        }
    }
}

public class SuppressionNestedInMemberAccess
{
    public void Method(string? a)
    {
        _ = a!.Length; // Compliant
    }

    public void MethodNarrowed(string? a)
    {
        if (a != null)
        {
            _ = a.Length; // Fixed
        }
    }
}

public class FieldSuppression
{
    private string? field;
    private string nonNullableField = "";

    public void Method()
    {
        _ = this.field!; // Compliant
        _ = this.nonNullableField; // Fixed
    }
}

public class FieldNarrowedByNullCheck
{
    private string? field;

    public void Method()
    {
        if (field != null)
        {
            _ = field; // Fixed
        }
    }
}

public class ValueTypes
{
    public void Method(int a)
    {
        _ = a; // Fixed
    }
}

public class UnconstrainedGeneric
{
    public void Method<T>(T a)
    {
        _ = a!; // Compliant, "T" is conservatively assumed to possibly be null
    }
}

public class NotNullConstrainedGeneric
{
    public void Method<T>(T a) where T : notnull
    {
        _ = a; // Fixed
    }
}

public class ObliviousType
{
    public void Method()
    {
        var a = Legacy.GetValue();
        _ = a; // Fixed
    }
}

#nullable disable
public class Legacy
{
    public static string GetValue() => null;
}
#nullable enable

public class NarrowedThenSuppressedInsideLocalFunction
{
    public void Method(string? a)
    {
        if (a != null)
        {
            Local();
        }

        void Local()
        {
            _ = a; // Fixed
        }
    }
}

public class NarrowedBySwitchPattern
{
    public void Method(string? a)
    {
        switch (a)
        {
            case not null:
                _ = a; // Fixed
                break;
        }
    }
}

public class NarrowedByNotNullWhen
{
    public void Method(string? a)
    {
        if (TryNormalize(a, out var result))
        {
            _ = result; // Fixed
        }
    }

    public static bool TryNormalize(string? input, [NotNullWhen(true)] out string? result)
    {
        result = input;
        return input != null;
    }
}

public class MultipleSuppressionsInSameStatement
{
    public void Method(string? a, string? b)
    {
        _ = a! + b!; // Compliant, neither "a!" nor "b!" narrows the other
    }
}

public class RefParameterReassigned
{
    public void Method(ref string? a)
    {
        a = "value";
        _ = a; // Fixed
    }
}

public class NarrowedBeforeAwait
{
    public async Task Method(string? a)
    {
        if (a != null)
        {
            await Task.Delay(1);
            _ = a; // Fixed
        }
    }
}

public class PropertyInitializer
{
    public string NameWarningsEnabled { get; set; } = default!; // Compliant, "default" for a non-nullable reference type is genuinely null
    public string TitleWarningsEnabled { get; set; } = null!; // Compliant, EF Core scaffolds required string properties this way
}

public class PragmaInsideExpression
{
    public void WarningsDisabledThenEnabled(bool condition, string? a)
    {
        if (a != null)
        {
#nullable disable warnings
            _ = (condition
                ? a
#nullable enable warnings
                : a); // Fixed
        }
    }

    public void DisableThenEnable(bool condition, string a)
    {
#nullable disable
        _ = (condition
            ? a
#nullable enable
            : "fallback"); // Fixed
    }
}

#endregion

#region S8970 - null-forgiving operator used where nullable warnings are disabled

public class IdentifierOperand
{
    public void Method()
    {
#nullable disable
        string a = null;
        _ = a; // Fixed
#nullable enable
    }
}

public class MemberAccessOperand
{
    public void Method(Uri uri)
    {
#nullable disable
        _ = uri.ToString(); // Fixed
#nullable enable
    }
}

public class IndexerOperand
{
    public void Method(string[] items)
    {
#nullable disable
        _ = items[0]; // Fixed
#nullable enable
    }
}

public class CastOperand
{
    public void Method(object o)
    {
#nullable disable
        _ = ((string)o); // Fixed
#nullable enable
    }
}

public class ParenthesizedOperand
{
    public void Method(string a)
    {
#nullable disable
        _ = (a ?? ""); // Fixed
#nullable enable
    }
}

public class LambdaOperand
{
    public void Method()
    {
#nullable disable
        Func<string, string> f = a => a; // Fixed
#nullable enable
    }
}

public class LocalFunctionOperand
{
    public void Method()
    {
#nullable disable
        string Local(string a) => a; // Fixed
#nullable enable
    }
}

public class FieldInitializer
{
#nullable disable
    private string field = null; // Fixed
#nullable enable
}

public class PropertyInitializerWarningsDisabled
{
#nullable disable
    public string NameWarningsDisabled { get; set; } = default; // Fixed
#nullable enable
}

public class PragmaInsideExpressionWarningsDisabled
{
    public void WarningsEnabledThenDisabled(bool condition, string? a)
    {
        if (a != null)
        {
            _ = (condition
                ? a
#nullable disable warnings
                : a); // Fixed
#nullable enable warnings
        }
    }

    public void EnableThenDisable(bool condition, string a)
    {
        _ = (condition
            ? a
#nullable disable
            : "fallback"); // Fixed
#nullable enable
    }
}

#endregion
