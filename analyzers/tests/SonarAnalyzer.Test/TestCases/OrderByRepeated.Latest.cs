using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    class OrderByRepeatedLatest
    {
        // Order() / OrderDescending() were introduced in .NET 7 and return IOrderedEnumerable<T>, so a second
        // ordering on their result is redundant just like a repeated OrderBy.
        public void OrderAndOrderDescending(int[] numbers)
        {
            numbers.Order().Order();                            // Noncompliant
            numbers.OrderDescending().OrderDescending();        // Noncompliant
            numbers.Order().OrderDescending();                  // Noncompliant
            numbers.OrderBy(x => x).Order();                    // Noncompliant
            numbers.Order().OrderBy(x => x);                    // Noncompliant
            numbers.Order();                                    // Compliant - single ordering
            numbers.Order().ThenBy(x => x);                     // Compliant - ThenBy is the recommended continuation
        }

        public void OrderOnQueryable(IQueryable<int> query)
        {
            query.Order().Order();                              // Noncompliant
            query.OrderDescending().OrderDescending();          // Noncompliant
            query.Order();                                      // Compliant
        }

        // Intermediate calls that do not preserve IOrderedEnumerable are known FNs; Skip/Take are true negatives.
        public void IntermediateCalls(int[] numbers)
        {
            numbers.OrderBy(x => x).Where(x => x > 0).OrderBy(x => x);   // FN
            numbers.OrderBy(x => x).Select(x => x).OrderBy(x => x);      // FN
            numbers.OrderBy(x => x).Skip(5).OrderBy(x => x);             // Compliant
            numbers.OrderBy(x => x).Take(3).OrderBy(x => x);             // Compliant
        }

        public void QueryComprehension(int[] numbers)
        {
            var consecutive =
                from x in numbers
                orderby x
                orderby x descending // Noncompliant
                select x;

            var singleClause =
                from x in numbers
                orderby x, x descending // Compliant
                select x;

            var separatedByWhere =
                from x in numbers
                orderby x
                where x > 0
                orderby x descending // Compliant
                select x;

            var single =
                from x in numbers
                orderby x // Compliant
                select x;
        }
    }
}
