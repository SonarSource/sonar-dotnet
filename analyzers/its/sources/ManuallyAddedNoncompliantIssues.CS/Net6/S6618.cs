using System;

namespace Net6;

public class S6618
{
    public void Method(string value, FormattableString formString)
    {
        FormattableString.CurrentCulture($"Value: {value}"); // Noncompliant (S6618)
    }
}
