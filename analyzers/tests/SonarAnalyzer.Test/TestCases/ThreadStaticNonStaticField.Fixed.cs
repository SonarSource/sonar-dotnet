using System;

namespace Tests.Diagnostics
{
    class XAttribute : Attribute { }
    public class ThreadStaticNonStaticField
    {
        private int count1 = 0, count11 = 0;

        [X]  // Fixed
        private int count2 = 0;

        [System.ThreadStatic]
        private static int count3 = 0;

        private int count4 = 0;
        private int count5;
        private int count6;
        private int count7;
    }

    public class ThreadStaticNonStaticFieldDerivedAttribute
    {
        [DerivedThreadStatic]  // FN for performance reasons we decided not to handle derived classes
        private int count1 = 0, count11 = 0;
    }

    public class DerivedThreadStaticAttribute : ThreadStaticAttribute { }
}
