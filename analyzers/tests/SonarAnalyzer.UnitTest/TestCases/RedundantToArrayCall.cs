using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class RedundantToArrayCall
    {
        public char[] ToCharArray()
        {
            return null;
        }

        public void Literals()
        {
            var c = "some string".ToCharArray()[10]; // Noncompliant, the indexer already returns a char
//                                ^^^^^^^^^^^
            c = "some string".ToCharArray(5, 4)[1];
            foreach (var v in "some string".ToCharArray()) // Noncompliant {{Remove this redundant 'ToCharArray' call.}}
            {
                // ...
            }

            var x = "some string".ToCharArray();

            c = this.ToCharArray()[10];
        }

        public void Coverage()
        {
            "x".ToString();
            this.Foo();
            foreach (var v in Foo()) { }
            Foo();
            var x = "x".ToCharArray(1, 2)[0]; // Compliant, it does a substring
        }

        public IEnumerable<string> Foo() => null;
    }
}
