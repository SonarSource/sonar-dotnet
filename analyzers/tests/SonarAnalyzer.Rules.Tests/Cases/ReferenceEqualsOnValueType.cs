using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class ReferenceEqualsOnValueType
    {
        public ReferenceEqualsOnValueType()
        {
            var b = object.ReferenceEquals(1, 2); //Noncompliant
//                  ^^^^^^^^^^^^^^^^^^^^^^
            b = ReferenceEquals(1, 2); //Noncompliant
            ReferenceEqualsOnValueType.ReferenceEquals(1, new object()); //Noncompliant {{Use a different kind of comparison for these value types.}}
            ReferenceEquals(new object(), new object());
        }
    }
}
