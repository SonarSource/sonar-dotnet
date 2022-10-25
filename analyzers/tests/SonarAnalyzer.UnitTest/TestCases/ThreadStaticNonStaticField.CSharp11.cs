using System;

namespace Tests.Diagnostics
{
    public class ThreadStaticNonStaticField
    {
        [ThreadStaticAttribute<int>]  // FN
        private int count1 = 0, count11 = 0;
    }

    public class ThreadStaticAttribute<T> : ThreadStaticAttribute { }
}
