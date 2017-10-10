using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    struct MyStruct // Noncompliant {{Implement 'IEquatable<T>' in value type 'MyStruct'.}}
//         ^^^^^^^^
    {
    }

    struct MyCompliantStruct : IEquatable<MyCompliantStruct> // Compliant
    {
        public bool Equals(MyCompliantStruct other)
        {
            throw new NotImplementedException();
        }
    }
}
