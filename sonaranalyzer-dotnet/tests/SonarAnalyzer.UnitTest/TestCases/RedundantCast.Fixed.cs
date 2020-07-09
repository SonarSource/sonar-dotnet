using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class RedundantCast
    {
        void foo(long l)
        {
            var x = new int[] {1, 2, 3}; // Fixed
            x = new int[] {1, 2, 3};
            x = x
; //Fixed
            x = x; //Fixed

            var y = x.OfType<object>();

            var zz = (int) l;
            int i = 0;
            var z = (int) i; // Fixed
            z = (Int32) i; // Fixed

            var w = (object) i;

            method(new int[] { 1, 2, 3 }); // Fixed
        }
        void method(IEnumerable<int> enumerable)
        { }

        void M()
        {
            var o = new object();
            var oo = o; // Fixed
            var i = o as RedundantCast; // Compliant
        }

        public void N(int[,] numbers)
        {
            numbers.Cast<int>().Where(x => x > 0); // Compliant, multidimensional arrays need to be cast
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/3273
        public void ReferenceTypes()
        {
            var stringArrayWithNull = new string[] { "one", "two", null, "three" };
            var filteredStringArray = stringArrayWithNull.OfType<string>(); // Compliant, has 3 items, without 'null'

            var enumerableOfObjects = new object[] { 1, "one", null };
            var filteredObjectArray = enumerableOfObjects.OfType<object>(); // Compliant, has 2 items, without 'null'
        }
    }
}
