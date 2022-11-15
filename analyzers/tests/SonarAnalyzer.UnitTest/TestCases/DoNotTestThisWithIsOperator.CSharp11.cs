using System;
using System.Collections;

namespace Tests.Diagnostics
{
    public class SomeClass : ArrayList
    {
        public void SomeMethod()
        {
            var a = this is [1, 2, 3]; // Noncompliant

            switch (this) // Noncompliant, switch statement
            {
                case [1, 2, 3]: // Secondary
                    return;
                case [4, 5, 6]: // Secondary
                    return;
                default:
                    break;
            }

            var res = this switch // Noncompliant, switch expression
            {
                [1, 2, 3] => 1, // Secondary
                [4, 5, 6] => 2, // Secondary
                _ => 42
            };
        }
    }
}
