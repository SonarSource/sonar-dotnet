using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Program
    {
        abstract class Base
        {
            public Base() // Noncompliant {{Change the visibility of this constructor to 'protected'.}}
//          ^^^^^^
            {
                //...
            }

            private Base() // Compliant
            {

            }

            protected Base() // Compliant
            {

            }

            internal Base() // Noncompliant
            {

            }

            internal protected Base() // Noncompliant
            {

            }
        }
    }
}