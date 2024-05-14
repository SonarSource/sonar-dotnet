using System;

namespace Tests.Diagnostics
{
    public class ThreadStaticNonStaticField
    {
        [ThreadStaticAttribute<int>]    // FN: for performance reasons we decided not to handle derived classes
        private int count1 = 0, count11 = 0;

        [ThreadStaticAttribute]         // Noncompliant
        private int count2 = 0;
    }

    public class ThreadStaticAttribute<T> : ThreadStaticAttribute { }
}
