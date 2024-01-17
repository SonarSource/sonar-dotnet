using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{

    class OrderByRepeated
    {
        public void Test()
        {
            new int[] { 1, 2, 3 }.OrderBy(i => i).OrderBy(i => i); //Noncompliant
//                                                ^^^^^^^
            new int[] { 1, 2, 3 }.OrderBy(i => i).ThenBy(i => i);
            new string[] { "" }
                .OrderBy(i => i, StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(i => i); //Noncompliant {{Use 'ThenBy' instead.}}
            new string[] { "" }
                .OrderBy(i => i, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(i => i, StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(i => i); //Noncompliant
        }

        public void Coverage()
        {
            new int[] { 1, 2, 3 }.OrderBy(i => i).Count();
            new int[] { 1, 2, 3 }.Select(i => i).OrderBy(i => i);
            new int[] { 1, 2, 3 }.OrderBy("x").ThenBy("x");
            new int[] { 1, 2, 3 }.OrderBy(i => i).ThenBy("x");
            new int[] { 1, 2, 3 }.OrderBy(i => i).OrderBy("x"); // Noncompliant
            new int[] { 1, 2, 3 }.OrderBy("x").ThenBy(i => i);
            var array = new int[] { 1, 2, 3 };
            MyExtensions.OrderBy(MyExtensions.OrderBy(array, "x"), "x");
            Foo();
        }

        public void Foo() { }
    }

    static class MyExtensions
    {
        public static IOrderedEnumerable<int> OrderBy(this IEnumerable<int> source, string x) => null;
        public static IEnumerable<int> ThenBy(this IEnumerable<int> source, string x) => null;
    }
}
