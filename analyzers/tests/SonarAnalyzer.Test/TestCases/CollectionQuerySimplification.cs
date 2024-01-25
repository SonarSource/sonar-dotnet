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
            var x3 = coll.Select(element => element as IList<int>).Any(element => element.Count != 0); // Compliant
            x = coll.Select((element) => ((element as object))).Any(element => (element != null) && CheckCondition(element) && true);  // Noncompliant use OfType
            x = coll.Select(element => ((element as object))).Any(element => (element != null) && CheckCondition(element) && true);  // Noncompliant use OfType
            var y = coll.Where(element => element is object).Select(element => element as object); // Noncompliant use OfType
//                       ^^^^^
            y = coll.Where(element => element is object).Select(element => element as object[]);
            y = coll.Where(element => element is object).Select(element => (object)element); // Noncompliant use OfType
            x = coll.Where(element => element == null).Any();  // Noncompliant use Any([expression])
//                   ^^^^^
            var z = coll.Where(element => element == null).Count();  // Noncompliant {{Drop 'Where' and move the condition into the 'Count'.}}
            z = Enumerable.Count(coll.Where(element => element == null));  // Noncompliant
            z = Enumerable.Count(Enumerable.Where(coll, element => element == null));  // Noncompliant
            y = coll.Select(element => element as object);
            y = coll.ToList().Select(element => element as object); // Noncompliant
            y = coll
                .ToList()  // Noncompliant {{Drop this useless call to 'ToList' or replace it by 'AsEnumerable' if you are using LINQ to Entities.}}
//               ^^^^^^
                .ToArray() // Noncompliant {{Drop this useless call to 'ToArray' or replace it by 'AsEnumerable' if you are using LINQ to Entities.}}
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
}
