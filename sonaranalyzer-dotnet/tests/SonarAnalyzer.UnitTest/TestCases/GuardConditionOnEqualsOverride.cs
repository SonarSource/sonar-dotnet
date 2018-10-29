using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Base
    {
        public override bool Equals(object other)
        {
            if (base.Equals(other)) // Okay; base is object
            {
                return true;
            }

            // do some checks here
            return false;
        }
    }
    class Derived : Base
    {
        public override bool Equals(object other)
        {
            if (base.Equals(other))  // Noncompliant {{Change this guard condition to call 'object.ReferenceEquals'.}}
//              ^^^^^^^^^^^^^^^^^^
            {
                return true;
            }

            // do some checks here
            return false;
        }
    }
}
