using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class EqualityCheckOnModulus
    {
        public void isOdd(int x)
        {
            var y = x%2 == 1; // Noncompliant; if x is negative, x % 2 == -1
//                  ^^^^^^^^
            y = x%2 != -1; // Noncompliant {{The result of this modulus operation may not be negative.}}
            y = 1 == x%2; // Noncompliant {{The result of this modulus operation may not be positive.}}
            y = x%2 != 0;
            y = Math.Abs(x%2) == 1;

            var unsignedY = 54U;

            var xx = unsignedY % 4 == 1; // Compliant

            var array = new[] {1};
            y = array.Length % 2 == 1; // Compliant: array Length is > 0;

            IEnumerable<int> enumerable = array;
            y = enumerable.Count() % 2 == 1; // Compliant - IEnumerable Count is > 0;
            y = enumerable.LongCount() % 2 == 1; // Compliant - IEnumerable LongCount is > 0;

            IEnumerable<long> enumerableLong = new List<long>();
            y = enumerableLong.Count() % 2 == 1; // Compliant - IEnumerable Count is > 0;
            y = enumerableLong.LongCount() % 2 == 1; // Compliant - IEnumerable LongCount is > 0;

            var list = new List<int>();
            y = list.Count % 2 == 1; // Compliant - Count property is > 0;
            y = list.Capacity % 2 == 1; // Compliant - Capacityproperty is > 0;

            var someString = "HelloImAWhale";
            y = someString.Length % 2 == 1; // Compliant - Length property is > 0;

            y = FakeCount() % 2 == 1; // Noncompliant

            y = (enumerableLong.Count() % 2) == 1; // Compliant
            y = 1 == (x % 2); // Noncompliant {{The result of this modulus operation may not be positive.}}
        }

        public int FakeCount() => -1;
    }
}
