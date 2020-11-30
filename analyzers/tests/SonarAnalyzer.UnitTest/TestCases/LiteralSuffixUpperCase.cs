using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class LiteralSuffixUpperCase
    {
        public void Test(long ui)
        {
            const long b = 0l;      // Noncompliant {{Upper case this literal suffix.}}
//                          ^
            const ulong c = 0Ul;
            const ulong d = 0uL;
            const decimal e = 1.2m;
            const float f = 1.2f;
            const double g = 1.2d;
            const double h = 1.2e-10f;
            const int i = 0xf; // Compliant
            const int j = 0Xf; // Compliant
            const int k = 0; // Compliant
            const uint l = 0u;

            Test(45l); // Noncompliant
        }
        public void TestOk()
        {
            const uint a = 0U;
            const long b = 100L;
            const ulong c = 0UL;
            const ulong d = 0UL;
            const decimal e = 1.2M;
            const float f = 1.2F;
            const double g = 1.2D;
            const double h = 1.2E-10;
            const int i = 0xF;
        }
    }
}
