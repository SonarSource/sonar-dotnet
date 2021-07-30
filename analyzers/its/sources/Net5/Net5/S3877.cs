using System;
using System.Runtime.CompilerServices;

namespace Net5
{
    public class S3877
    {
        [ModuleInitializer]
        internal static void M1()
        {
            throw new Exception(); // Noncompliant
        }

        [ModuleInitializer]
        internal static void M2() => throw new Exception(); // Noncompliant
    }
}
