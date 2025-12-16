using System;

namespace Tests.Diagnostics
{
    class XAttribute : Attribute { }

    record Record
    {
        private int count1 = 0, count11 = 0;

        [X]  // Fixed
        private int count2 = 0;

        [System.ThreadStatic]
        private static int count3 = 0;

        private int count4 = 0;
    }

    record struct StructRecord
    {
        public StructRecord() { }
        private int count1 = 0, count11 = 0;

        [X]  // Fixed
        private int count2 = 0;

        [System.ThreadStatic]
        private static int count3 = 0;

        private int count4 = 0;
    }

    public class ThreadStaticNonStaticField
    {
        [ThreadStaticAttribute<int>]    // FN: for performance reasons we decided not to handle derived classes
        private int count1 = 0, count11 = 0;
        private int count2 = 0;
    }

    public class ThreadStaticAttribute<T> : ThreadStaticAttribute { }
}
