using System;

namespace Tests.Diagnostics
{
    class XAttribute : Attribute { }

    record Record
    {
        [ThreadStatic]  // Noncompliant
    //   ^^^^^^^^^^^^
        private int count1 = 0, count11 = 0;

        [ThreadStatic, X]  // Noncompliant {{Remove the 'ThreadStatic' attribute from this definition.}}
        private int count2 = 0;

        [System.ThreadStatic]
        private static int count3 = 0;

        private int count4 = 0;
    }

    record struct StructRecord
    {
        public StructRecord() { }

        [ThreadStatic]  // Noncompliant
    //   ^^^^^^^^^^^^
        private int count1 = 0, count11 = 0;

        [ThreadStatic, X]  // Noncompliant {{Remove the 'ThreadStatic' attribute from this definition.}}
        private int count2 = 0;

        [System.ThreadStatic]
        private static int count3 = 0;

        private int count4 = 0;
    }

    public class ThreadStaticNonStaticField
    {
        [ThreadStaticAttribute<int>]    // FN: for performance reasons we decided not to handle derived classes
        private int count1 = 0, count11 = 0;

        [ThreadStaticAttribute]         // Noncompliant
        private int count2 = 0;
    }

    public class ThreadStaticAttribute<T> : ThreadStaticAttribute { }
}
