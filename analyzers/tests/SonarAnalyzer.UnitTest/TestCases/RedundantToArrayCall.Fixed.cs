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
            var c = "some string"[10]; // Fixed
            c = "some string".ToCharArray(5, 4)[1];
            foreach (var v in "some string") // Fixed
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
