using System;
using System.Collections.Generic;

class CSharp12
{
    void CollectionExpressions(string s1, string s2)
    {
        IList<string> l;
        l = [string.Format("foo")];                                                // Noncompliant
        l = [string.Format("{0}", s1, s2), string.Format(format: "{0}", arg0: 1)]; // Noncompliant
        //   ^^^^^^^^^^^^^
        l = [string.Format("{-1}", s1), string.Format(null, "{}")];
        //   ^^^^^^^^^^^^^
        //                              ^^^^^^^^^^^^^@-1
    }
}

class CSharp13
{
    void EscapeSequence()
    {
        _ = string.Format("{'\e'}");        // Noncompliant
        _ = string.Format("{{\e{0}}}", 42); // Compliant
        _ = string.Format("{0}", "\e");     // Compliant
    }
}

class CSharp15
{
    void CollectionExpressionArguments(object first, object second, object third)
    {
        _ = string.Format("{0} {1}", [first, second]);                         // Noncompliant {{Invalid string format, the highest string format item index should not be greater than the arguments count.}} FP NET-5012
        _ = string.Format("{0} {1}", new object[] { first, second });          // Compliant
        _ = string.Format("{0} {2}", [first, second, third]);                  // Noncompliant {{Invalid string format, the highest string format item index should not be greater than the arguments count.}} FP NET-5012, FN NET-5013
        _ = string.Format("{0} {2}", new object[] { first, second, third });   // Noncompliant {{The format string might be wrong, the following item indexes are missing: '1'.}}
        _ = string.Format("{0}", [first]);                                     // Compliant
    }
}
