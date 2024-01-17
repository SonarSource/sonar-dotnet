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
            2147483599 => i + 100,  // FN
            _ => i + 100,           // Compliant
        };
    }

    public void PropertyPattern(string s)
    {
        if (s is { Length: 2147483600 })
            _ = s.Length + 100; // FN due to the lack of property sensitivity
    }
}
