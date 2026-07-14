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
        _ = a!; // Noncompliant {{Remove this null-forgiving operator; the compiler already knows this expression is not null here.}}
//           ^
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
            _ = a!; // Noncompliant
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
        _ = a!; // Noncompliant
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
            _ = a!.Length; // Noncompliant
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
        _ = this.nonNullableField!; // Noncompliant
    }
}

public class ValueTypes
{
    public void Method(int a)
    {
        _ = a!; // Noncompliant, value types are never null
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
        _ = a!; // Noncompliant, the "notnull" constraint already guarantees "a" cannot be null
    }
}

public class ObliviousType
{
    public void Method()
    {
        var a = Legacy.GetValue();
        _ = a!; // Noncompliant, the oblivious source never triggers a nullable warning
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
            _ = a!; // Noncompliant
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
                _ = a!; // Noncompliant
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
            _ = result!; // Noncompliant
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
        _ = a!; // Noncompliant
    }
}

public class NarrowedBeforeAwait
{
    public async Task Method(string? a)
    {
        if (a != null)
        {
            await Task.Delay(1);
            _ = a!; // Noncompliant
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
                : a)!; // Noncompliant, warnings are enabled again at the null-forgiving operator and the compiler already knows this is not null
        }
    }

    public void DisableThenEnable(bool condition, string a)
    {
#nullable disable
        _ = (condition
            ? a
#nullable enable
            : "fallback")!; // Noncompliant, warnings are enabled again and the compiler already knows this is not null
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
        _ = a!; // Noncompliant {{Remove this null-forgiving operator; nullable warnings are disabled here.}}
//           ^
#nullable enable
    }
}

public class MemberAccessOperand
{
    public void Method(Uri uri)
    {
#nullable disable
        _ = uri.ToString()!; // Noncompliant
#nullable enable
    }
}

public class IndexerOperand
{
    public void Method(string[] items)
    {
#nullable disable
        _ = items[0]!; // Noncompliant
#nullable enable
    }
}

public class CastOperand
{
    public void Method(object o)
    {
#nullable disable
        _ = ((string)o)!; // Noncompliant
#nullable enable
    }
}

public class ParenthesizedOperand
{
    public void Method(string a)
    {
#nullable disable
        _ = (a ?? "")!; // Noncompliant
#nullable enable
    }
}

public class LambdaOperand
{
    public void Method()
    {
#nullable disable
        Func<string, string> f = a => a!; // Noncompliant
#nullable enable
    }
}

public class LocalFunctionOperand
{
    public void Method()
    {
#nullable disable
        string Local(string a) => a!; // Noncompliant
#nullable enable
    }
}

public class FieldInitializer
{
#nullable disable
    private string field = null!; // Noncompliant
#nullable enable
}

public class PropertyInitializerWarningsDisabled
{
#nullable disable
    public string NameWarningsDisabled { get; set; } = default!; // Noncompliant
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
                : a)!; // Noncompliant, warnings are disabled at the null-forgiving operator
#nullable enable warnings
        }
    }

    public void EnableThenDisable(bool condition, string a)
    {
        _ = (condition
            ? a
#nullable disable
            : "fallback")!; // Noncompliant, warnings are disabled at the null-forgiving operator
#nullable enable
    }
}

#endregion
