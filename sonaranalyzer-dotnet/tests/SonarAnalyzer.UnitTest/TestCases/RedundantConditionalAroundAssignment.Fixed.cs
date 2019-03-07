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

            if (Property != 42)
            {
                Property = 42;
            }
        }

        private static int? f;
        // Do not report issue on field check within a property accessor as it might be expensive to set again the value
        public static int Property
        {
            get
            {
                f = null;

                return 1;
            }
            set
            {
                if (f != null)
                {
                    f = null;
                }
            }
        }
    }
}
