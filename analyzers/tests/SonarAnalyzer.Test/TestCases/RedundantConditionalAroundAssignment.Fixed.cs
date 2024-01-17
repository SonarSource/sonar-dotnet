namespace Tests.Diagnostics
{
    public static class RedundantConditionalAroundAssignment
    {
        public static void Test()
        {
            var x = "";

            if (x == "")
            {
                x = "";
            }

            x = null;

            if (!(x is null)) // FN (is expression not supported yet)
            {
                x = null;
            }

            if (x is null) // Compliant
            {
                x = null;
            }

            if (x != null) // Compliant FN
                x = null;

            x = null;

            x = null;

            x = (null);

            object a = null;
            switch (a)
            {
                case null:   // Compliant FN
                    {
                        a = null;
                    }
                    break;
                default:
                    break;
            }

            if (null != x)
            {
                x = null;
                x = "";
            }

            if (null != x)
            {
                x += "";
            }

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

            if (null != x)
            {
                Test();
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

            int i = 0;
            if (i++)   // Error [CS0029]
            {
                i = 0;
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
