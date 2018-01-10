using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class XAttribute : Attribute { }
    public class ThreadStaticNonStaticField
    {
        private int count1 = 0, count11 = 0;

        [X]  // Fixed
        private int count2 = 0;

        [System.ThreadStatic]
        private static int count2 = 0;

        private int count3 = 0;
    }
}
