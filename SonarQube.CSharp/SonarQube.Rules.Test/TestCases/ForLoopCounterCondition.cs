namespace Tests.Diagnostics
{
    public class ForLoopCounterCondition
    {
        private int someMethod(HelperClass o)
        {
            return o.Property;
        }

        public class HelperClass
        {
            public int Property { get; set; }
        }

        public int P;
        public ForLoopCounterCondition()
        {
            int k, j = 3;
            for (int i = 0; i < 10; i++)
            {

            }
            for (P = 0; P < 10; P++)
            {

            }

            for (int a = 0, b = 5; a + b < 10; a++, b += a)
            {
                //do some stuff
            }

            for (int i = 0; i < 5; )
            {

            }

            for (int i = 0; i < 10; j++) //Noncompliant
            {

            }
            for (int i = 0; ; i++) //Noncompliant
            {

            }

            var o = new HelperClass();
            for (k = 0; someMethod(o) < 10; P++, j++, o.Property--, someMethod(o)) //Compliant
            {
                o.Property = P;
            }

            for (k = 0; k < 10; o.Property--) //Noncompliant
            {
                o.Property = P;
            }

            for (k = 0; someMethod(o) < 10; o.Property--)
            {
                o.Property = P;
            }
        }
    }
}
