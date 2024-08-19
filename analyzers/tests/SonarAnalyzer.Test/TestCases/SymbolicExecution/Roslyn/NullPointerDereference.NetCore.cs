using System;

class NullPointerDereference
{
    public void Type_IsAssignableFrom_LearnsPossibleNull(Type type, Type other)
    {
        if (other.IsAssignableFrom(type))   // Decorated with [NotNullWhenAttribute(true)]
        {
            type.ToString();
        }
        else
        {
            type.ToString();    // FN? It could have been null in theory
        }
    }
}

public interface IWithDefaultMembers
{
    string NoncompliantDefaultInterfaceMethod(string obj) =>
        obj != null ? null : obj.ToLower(); // Noncompliant

    string CompliantDefaultInterfaceMethod(string obj) =>
        obj == null ? null : obj.ToLower();
}

public class ThrowHelper
{
    public void DoesNotReturnIsRespectedOutsideNullableContext()
    {
        object o = null;
        DoesNotReturn();
        o.ToString(); // Compliant. Unreachable
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public void DoesNotReturn() { }
}

public class NotNullWhenTests
{
    public void TryParseNull()
    {
        string boolString = null;
        if (bool.TryParse(boolString, out var result)) // public static bool TryParse([NotNullWhen(true)] string? value, out bool result)
        {
            boolString.ToString(); // Compliant We know that boolString is not null here
        }
        else
        {
            boolString.ToString(); // Noncompliant
        }
    }

    public void TryParseNotNull()
    {
        string boolString = "something";
        if (bool.TryParse(boolString, out var result))
        {
            boolString.ToString(); // Compliant
        }
        else
        {
            boolString.ToString(); // Compliant
        }
    }

    public void TryParseUnknown(string boolString)
    {
        if (bool.TryParse(boolString, out var result))
        {
            boolString.ToString(); // Compliant
        }
        else
        {
            boolString.ToString(); // FN? [NotNullWhen(true)] suggests that parsing may have failed because boolString was null
        }
    }

    public void CustomTryUpper(string text)
    {
        if (TryToUpper(text, out var result))
        {
            text.ToString(); // Compliant
        }
        else
        {
            text.ToString(); // FN? [NotNullWhen(true)] suggests that parsing may have failed because text was null
        }
    }

    private static bool TryToUpper([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string someValue, out string result)
    {
        result = someValue?.ToUpper();
        return !string.IsNullOrEmpty(someValue);
    }

    public void TwoParameterTest(string first, string second)
    {
        if (TwoNotNullWhen(first, second))
        {
            first.ToString();   // Compliant
            second.ToString();  // Compliant
        }
        else
        {
            first.ToString();   // Compliant, we don't learn Null constraint from NotNullWhenAttribute
            second.ToString();  // Compliant, we don't know value of this
        }
    }

    private static bool TwoNotNullWhen([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string first, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string second) => true;
}
