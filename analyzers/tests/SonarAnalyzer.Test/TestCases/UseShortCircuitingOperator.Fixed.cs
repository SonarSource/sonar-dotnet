using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class UseShortCircuitingOperator
    {
        public UseShortCircuitingOperator()
        {
            var b = true || false;   // Fixed
            b = true && false;       // Fixed
            b = true && false;

            var i = 1 | 2;
        }
    }
}
