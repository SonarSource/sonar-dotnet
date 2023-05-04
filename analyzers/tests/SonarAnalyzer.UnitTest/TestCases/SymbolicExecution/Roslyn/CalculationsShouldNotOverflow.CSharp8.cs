using System;
using System.Collections.Generic;
using System.Linq;

public class NativeInt
{
    public void SwitchExpression(int i)
    {
        _ = i switch
        {
            2147483547 => i + 100,  // Compliant
            _ => i + 100,           // FIXME Non-compliant
        };
    }

    public void PropertyPattern(string s)
    {
        if (s is { Length: 2147483600 })
            _ = s.Length + 100; // FIXME Non-compliant
    }
}
