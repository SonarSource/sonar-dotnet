using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    // Repro for: https://github.com/SonarSource/sonar-dotnet/issues/5219
    public class Repro
    {
        void Reproducers(bool condition)
        {
            bool? v1 = condition ? true : null;

            bool? v2 = condition ? null : true;

            bool? v3 = condition ? true : SomeMethod();

            bool? v4 = condition || SomeMethod2(); // Fixed

            bool? v5 = condition || SomeMethod2();
        }

        public bool? SomeMethod()
        {
            return null;
        }

        public bool SomeMethod2()
        {
            return true;
        }

        // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7688
        void IsNotPattern(bool a, bool? b)
        {
            var z = true is not false;  // Compliant FN
            z = false is not true;      // Compliant FN

            if (a is not true) // Compliant FN
            { }
            if (b is not true) // Compliant
            { }

            const bool c = true;
            a = a is not c;
            a = (a is not c) ? a : c;
            a = (a is not c && a) ? a : c;
            a = a is not c && a;

            var x = a is not true ? throw new Exception() : false;
        }
    }
}
