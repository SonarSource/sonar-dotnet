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
