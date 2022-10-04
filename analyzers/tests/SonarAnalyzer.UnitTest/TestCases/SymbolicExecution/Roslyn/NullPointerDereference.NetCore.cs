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
            type.ToString();    // Noncompliant, could be unassignable because it was null
        }
    }
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
            boolString.ToString(); // Noncompliant. [NotNullWhen(true)] suggests that parsing may have failed because boolString was null
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
            text.ToString(); // Noncompliant.
        }
    }

    private static bool TryToUpper([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string someValue, out string result)
    {
        result = someValue?.ToUpper();
        return !string.IsNullOrEmpty(someValue);
    }
}
