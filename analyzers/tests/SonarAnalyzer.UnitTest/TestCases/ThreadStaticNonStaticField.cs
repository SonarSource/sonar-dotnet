using System;

namespace Tests.Diagnostics
{
    class XAttribute : Attribute { }
    public class ThreadStaticNonStaticField
    {
        [ThreadStatic]  // Noncompliant
//       ^^^^^^^^^^^^
        private int count1 = 0, count11 = 0;

        [ThreadStatic, X]  // Noncompliant {{Remove the 'ThreadStatic' attribute from this definition.}}
        private int count2 = 0;

        [System.ThreadStatic]
        private static int count3 = 0;

        private int count4 = 0;

        [ThreadStaticAttribute]  // Noncompliant
        private int count5;

        [System.ThreadStaticAttribute]  // Noncompliant
        private int count6;

        [System.ThreadStatic]  // Noncompliant
        private int count7;
    }

    public class ThreadStaticNonStaticFieldDerivedAttribute
    {
        [DerivedThreadStatic]  // FN for performance reasons we decided not to handle derived classes
        private int count1 = 0, count11 = 0;
    }

    public class DerivedThreadStaticAttribute : ThreadStaticAttribute { }
}
