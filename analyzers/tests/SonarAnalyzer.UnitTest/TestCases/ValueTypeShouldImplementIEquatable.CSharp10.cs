using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    record struct MyStruct // Compliant. Record struct implement IEquatable by definition
    {
    }

    record struct MyCompliantStruct : IEquatable<MyCompliantStruct> // Compliant
    {
        public bool Equals(MyCompliantStruct other)
        {
            throw new NotImplementedException();
        }
    }
}
