using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Tests.Diagnostics
{
    public class CollectionQuerySimplification
    {
        public CollectionQuerySimplification(List<object> coll)
        {
            var x = coll.Select(element => element as object).Any(element => element != null);  // Noncompliant {{Use 'OfType<object>()' here instead.}}
//                       ^^^^^^
            var x4 = coll.Select(element => element as object).Any(element => element == null);
            var x2 = coll.Select(element => element as object).Any(element => null != element);  // Noncompliant {{Use 'OfType<object>()' here instead.}}
            //            ^^^^^^

            _ = coll?.Select(element => element as object).Any(element => null != element);  // Noncompliant
            _ = coll.Select(element => element as object)?.Any(element => null != element);  // Noncompliant

            var x3 = coll.Select(element => element as IList<int>).Any(element => element.Count != 0); // Compliant
            x = coll.Select((element) => ((element as object))).Any(element => (element != null) && CheckCondition(element) && true);  // Noncompliant use OfType
            x = coll.Select(element => ((element as object))).Any(element => (element != null) && CheckCondition(element) && true);  // Noncompliant use OfType
            var y = coll.Where(element => element is object).Select(element => element as object); // Noncompliant use OfType
//                       ^^^^^
            y = coll.Where(element => element is object).Select(element => element as object[]);
            y = coll.Where(element => element is object).Select(element => (object)element); // Noncompliant use OfType

            // Verbatim identifier on the reference side (@element) vs the plain parameter (element): the operand's
            // ValueText ("element") matches the lambda parameter, while the old ToString() ("@element") comparison
            // would miss it. Each case isolates one of the migrated matching spots (previously a false negative).
            x = coll.Select(element => element as object).Any(element => @element != null);    // Noncompliant {{Use 'OfType<object>()' here instead.}}
            y = coll.Where(element => element is object).Select(element => @element as object); // Noncompliant use OfType
            y = coll.Where(element => element is object).Select(element => (object)@element);   // Noncompliant use OfType

            x = coll.Where(element => element == null).Any();  // Noncompliant use Any([expression])
//                   ^^^^^
            var z = coll.Where(element => element == null).Count();  // Noncompliant {{Drop 'Where' and move the condition into the 'Count'.}}
            z = Enumerable.Count(coll.Where(element => element == null));  // Noncompliant
            z = Enumerable.Count(Enumerable.Where(coll, element => element == null));  // Noncompliant
            y = coll.Select(element => element as object);
            y = coll.ToList().Select(element => element as object); // Noncompliant
            y = coll
                .ToList()  // Noncompliant {{Drop this useless call to 'ToList'.}}
//               ^^^^^^
                .ToArray() // Noncompliant {{Drop this useless call to 'ToArray'.}}
                .Select(element => element as object);

            y = coll
                .AsEnumerable()
                .Where(e => e == null);

            var z2 = coll
                .Select(element => element as object)
                .ToList();

            var c = coll.Count(); //Noncompliant
//                       ^^^^^
            c = coll.OfType<object>().Count();

            x = Enumerable.Select(coll, element => element as object).Any(element => element != null); //Noncompliant
            x = Enumerable.Any(Enumerable.Select(coll, element => element as object), element => element != null); //Noncompliant

            coll.ToList().AsEnumerable(); // Compliant, we ignore AsEnumerable() as it is somewhat cleaner way to cast to IEnumerable<T> and has no side effects
        }

        public bool CheckCondition(object x)
        {
            return true;
        }

        public void Method(IEnumerable<int> ints)
        {
            var x = ints.ToList().AsReadOnly(); // compliant, AsReadOnly is defined on List<>
        }
    }

    public partial struct SyntaxList<TNode> : IReadOnlyList<TNode>, IEquatable<SyntaxList<TNode>>
    {
        public int Count => 0;

        public TNode this[int index] => default(TNode);

        public void Method(IEnumerable<TNode> ints)
        {
            CreateList(ints.Where(x => true).ToList());
        }
        private static SyntaxList<TNode> CreateList(List<TNode> items) => default(SyntaxList<TNode>);

        public IEnumerator<TNode> GetEnumerator() => null;

        IEnumerator IEnumerable.GetEnumerator() => null;

        public bool Equals(SyntaxList<TNode> other) => true;
    }

    public class IQueryableTests
    {
        public void Test(IQueryable<object> coll)
        {
            var x = coll.Select(element => element as object).Any(element => element != null);          // Noncompliant {{Use 'OfType<object>()' here instead.}}
//                       ^^^^^^
            var x4 = coll.Select(element => element as object).Any(element => element == null);
            var x2 = coll.Select(element => element as object).Any(element => null != element);         // Noncompliant {{Use 'OfType<object>()' here instead.}}
            //            ^^^^^^
            var x3 = coll.Select(element => element as IList<int>).Any(element => element.Count != 0);  // Compliant
            x = coll.Select((element) => ((element as object))).Any(element => (element != null) && CheckCondition(element) && true);   // Noncompliant use OfType
            x = coll.Select(element => ((element as object))).Any(element => (element != null) && CheckCondition(element) && true);     // Noncompliant use OfType
            var y = coll.Where(element => element is object).Select(element => element as object);      // Noncompliant use OfType
//                       ^^^^^
            y = coll.Where(element => element is object).Select(element => element as object[]);
            y = coll.Where(element => element is object).Select(element => (object)element);            // Noncompliant use OfType

            x = coll.Select(element => element as object).Any(element => @element != null);             // Noncompliant {{Use 'OfType<object>()' here instead.}}
            y = coll.Where(element => element is object).Select(element => @element as object);         // Noncompliant use OfType
            y = coll.Where(element => element is object).Select(element => (object)@element);           // Noncompliant use OfType

            x = coll.Where(element => element == null).Any();                                           // Noncompliant use Any([expression])
//                   ^^^^^
            var z = coll.Where(element => element == null).Count();                                     // Noncompliant {{Drop 'Where' and move the condition into the 'Count'.}}
            z = Enumerable.Count(coll.Where(element => element == null));                               // Noncompliant
            z = Enumerable.Count(Enumerable.Where(coll, element => element == null));                   // Noncompliant
            y = coll.Select(element => element as object);

            var z2 = coll
                .Select(element => element as object)
                .ToList();

            var midChain = coll
                .ToList()   // Compliant - materializing an IQueryable (eager) is not equivalent to keeping it deferred
                .Select(element => element as object);

            var c = coll.Count(); // Compliant - Count() is never flagged on an IQueryable source

            c = coll.OfType<object>().Count();

            x = Enumerable.Select(coll, element => element as object).Any(element => element != null);              //Noncompliant
            x = Enumerable.Any(Enumerable.Select(coll, element => element as object), element => element != null);  //Noncompliant

            coll.ToList().AsEnumerable(); // Compliant, we ignore AsEnumerable() as it is somewhat cleaner way to cast to IEnumerable<T> and has no side effects        }
        }

        public bool CheckCondition(object x)
        {
            return true;
        }

        public void Method(IEnumerable<int> ints)
        {
            var x = ints.ToList().AsReadOnly(); // compliant, AsReadOnly is defined on List<>
        }
    }

    // A provider-specific IQueryable that also exposes a 'Count' property: calling 'Count()' may re-execute the query
    // while the property may return a pre-fetched total, so the two are not interchangeable.
    public interface ICustomQueryable<T> : IQueryable<T>
    {
        int Count { get; }
    }

    public class CustomIQueryableWithCountProperty
    {
        public void Test(ICustomQueryable<object> coll)
        {
            var c = coll.Count();                               // Compliant - Count() on an IQueryable is never flagged
            var filtered = coll.ToList().Where(x => x != null); // Compliant - materializing an IQueryable is not equivalent to deferred execution
        }
    }
}

namespace CollectionQuerySimplificationCoverage
{
    public static class CustomLinq
    {
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<bool> predicate) => source;
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, T[] filter) => source;
        public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, int extra) => new List<TResult>();
        public static List<T> Take<T>(this List<T> source, int count) => source;
    }

    public class EdgeCases
    {
        public void EdgeCasesWithCustomLinq(List<object> coll)
        {
            object other = null;

            _ = CustomLinq.Take(coll.ToList(), 1);                                              // Noncompliant {{Drop this useless call to 'ToList'.}}
            _ = coll.Where(new object[0]).Any();                                                // Compliant
            _ = coll.ToList().Contains(null);                                                   // Compliant
            _ = coll.Select(element => element as object).First();                              // Compliant
            _ = coll.Select(element => element as object).Any(element => Keep(element));        // Compliant
            _ = coll.Where(() => other is object).Select(element => element as object);         // Compliant
            _ = coll.Where(element => element is object).Select(element => (object)element, 0); // Compliant
        }
        private bool Keep(object element) => element != null;
    }

    public class ImplicitReceiver : List<object>
    {
        public void ToCollectionCallWithoutReceiver()
        {
            _ = ToArray().Select(element => element as object); // Noncompliant {{Drop this useless call to 'ToArray'.}}
        }
    }
}
