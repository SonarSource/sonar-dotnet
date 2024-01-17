using System;

namespace Tests.Diagnostics
{
    class XAttribute : Attribute { }
    record struct ThreadStaticNonStaticField
    {
        public ThreadStaticNonStaticField() { }
        private int count1 = 0, count11 = 0;

        [X]  // Fixed
        private int count2 = 0;

        [System.ThreadStatic]
        private static int count3 = 0;

        private int count4 = 0;
    }
}
