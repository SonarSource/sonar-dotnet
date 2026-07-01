using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class EnumerableSumInUnchecked
    {
        public void Test(List<int> list, List<float> list2)
        {
            int a = list.Sum();  // Compliant

            int d = unchecked(list.Sum());  // Noncompliant {{Refactor this code to handle 'OverflowException'.}}
//                                 ^^^

            unchecked
            {
                int e = list.Sum();  // Noncompliant
//                           ^^^

                e = Enumerable.Sum(list); // Noncompliant
//                             ^^^

                float floatSum = list2.Sum(); // Compliant
            }

            checked
            {
                int e = list.Sum();  // Compliant
            }

            unchecked
            {
                try
                {
                    int e = list.Sum();
                }
                catch (System.OverflowException e)
                {
                    // exception handling...
                }
            }

            var l = new List<double>();
            unchecked
            {
                var x = l.Sum();  // Compliant, it's on double
            }

            var l2 = new List<Nullable<long>>();
            unchecked
            {
                var y = l2.Sum(ll => ll);  // Noncompliant
            }

            // coverage
            list.Count();
            MySum();
            unchecked
            {
                MySum();
            }
        }

        void MySum() { }
    }

    public class QueryableSumInUnchecked
    {
        public void Test(IQueryable<int> queryable, IQueryable<float> queryable2)
        {
            int a = queryable.Sum();  // Compliant

            int d = unchecked(queryable.Sum());  // Noncompliant {{Refactor this code to handle 'OverflowException'.}}
//                                      ^^^

            unchecked
            {
                int e = queryable.Sum();  // Noncompliant
//                                ^^^

                e = Enumerable.Sum(queryable); // Noncompliant
//                             ^^^

                float floatSum = queryable2.Sum(); // Compliant
            }

            checked
            {
                int e = queryable.Sum();  // Compliant
            }

            unchecked
            {
                try
                {
                    int e = queryable.Sum();
                }
                catch (System.OverflowException e)
                {
                    // exception handling...
                }
            }

            var l = new List<double>().AsQueryable();
            unchecked
            {
                var x = l.Sum();  // Compliant, it's on double
            }

            var l2 = new List<Nullable<long>>().AsQueryable();
            unchecked
            {
                var y = l2.Sum(ll => ll);  // Noncompliant
            }

            // coverage
            queryable.Count();
            MySum();
            unchecked
            {
                MySum();
            }
        }

        void MySum() { }
    }
}
