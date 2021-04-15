using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Tests.Diagnostics
{
    [Pure]
    public class PureType
    {
        public static int GetValue() => 42;
    }

    public class NonPureType
    {
        public static int GetValue() => 42;
    }

    class ReturnValueIgnored
    {
        [Pure]
        int Method() { return 0; }
        [Pure]
        int Method(ref int i) { return i; }

        void Test()
        {
            new int[] { 1 }.Where(i => true); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            var k = 0;
            new int[] { 1 }.Where(i => { k++; return true; }); // Noncompliant, although it has side effect

            new int[] { 1 }.ToList().ForEach(i => { });

            new int[] { 1 }.ToList(); // Noncompliant {{Use the return value of method 'ToList'.}}
            new int[] { 1 }.OfType<object>(); // Noncompliant

            "this string".Equals("other string"); // Noncompliant
            M("this string".Equals("other string"));

            "this string".Equals(new object()); // Noncompliant
            Method(); // Noncompliant
//          ^^^^^^^^

            1.ToString(); // Noncompliant

            int j = 1;
            Method(ref j);

            Action<int> a = (input) => "this string".Equals("other string"); // Noncompliant
            Func<int, bool> f = (input) => "this string".Equals("other string");

            PureType.GetValue(); // Noncompliant
            NonPureType.GetValue();

            "".DoSomething(null); // Compliant
            new int[] { 1 }.DoSomething(null); // Compliant

            string.Intern("abc"); // Noncompliant {{Use the return value of method 'Intern'.}}
            string.Compare("abc", "def"); // Noncompliant
        }
        void M(object o) { }
    }

    public static class Ext
    {
        public static int DoSomething<T>(this IEnumerable<T> self, Action action) { return 42; }
        public static int DoSomething(this string self, Action action) { return 42; }
    }
}
