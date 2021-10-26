using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    record struct MyStruct // FN
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
