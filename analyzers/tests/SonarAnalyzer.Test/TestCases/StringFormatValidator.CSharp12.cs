using System;
using System.Collections.Generic;

class CollectionExpressions
{
    void Test(string s1, string s2)
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
