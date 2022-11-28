using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class RedundantToCharArrayCall
    {
        public void CreateNew2(int propertyValue)
        {
            var c = "some string"u8[10];        // Fixed
            c = "some string"u8.Slice(5, 4)[1];           // Compliant
            foreach (var v in "some string"u8)  // Fixed
            {
                // ...
            }

            var x = "some string"u8.ToArray();
        }
    }
}
