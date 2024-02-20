using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLatest.CSharp9Features;

public class S4201
{
    public bool Method(Apple apple, object n, object m)
    {
        // Noncompliant
        var r1 = (n is string && n is not null);
        var r2 = n switch
        {
            Apple appleInside and not null => true,
            _ => false
        };
        var r3 = n is null || !(n is Apple);
        var r4 = m switch
        {
            null or not "a" => true,
            _ => false
        };

        // Compliant
        var r5 = apple != null && apple is not { Taste: "Sweet", Color: "Red" };
        switch (n)
        {
            case not null or Apple:
                return true;
            default:
                break;
        }

        return r1 && r2 && r3 && r4 && r5;
    }
}
