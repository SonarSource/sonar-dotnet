using System;

namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        static virtual string MyStaticVirtualMethod(bool condition)
        {
            if (condition)  // Noncompliant
            return "Noncompliant";   // Secondary

            if (condition) // Compliant
                return "Compliant";

            return string.Empty;
        }
    }
}
