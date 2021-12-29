using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Program
    {
        abstract class Base
        {
            public Base() // Noncompliant {{Change the visibility of this constructor to 'private', 'private protected' or 'protected'.}}
//          ^^^^^^
            {
                //...
            }

            private Base(int i) // Compliant
            {

            }

            protected Base(int i, int j) // Compliant
            {

            }

            internal Base(int i, int j, int k) // Noncompliant {{Change the visibility of this constructor to 'private' or 'private protected'.}}
            {

            }

            internal protected Base(int i, int j, int k, int l) // Noncompliant
            {

            }
        }
    }
}
