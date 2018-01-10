using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public static class RedundantConditionalAroundAssignment
    {
        public static void Test()
        {
            var x = "";

            x = null;

            x = null;

            x = null;

            x = (null);

            if ((null != x))
            {
                x = (null);
            }
            else
            { }

            if (true)
            { }
            else if ((null != x))
            {
                x = (null);
            }

            if (null != x)
            {
                x = "";
            }

            if (null == x)
            {
                x = null;
            }

            var y = 1;
            if (y == 2)
            {
                y += 2;
            }
        }
    }
}
