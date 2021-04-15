using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    record MyType
    {
        public static MyType operator ==(MyType x, MyType y) // Error [CS0111]
        {
            return null;
        }

        public static MyType operator !=(MyType x, MyType y) => null; // Error [CS0111]
    }
}
