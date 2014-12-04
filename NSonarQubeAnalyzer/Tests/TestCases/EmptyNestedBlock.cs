using System;

namespace Tests.Diagnostics
{
    public class EmptyNestedBlock
    {
        public EmptyNestedBlock()
        {
        }

        public ~EmptyNestedBlock()
        {
        }

        void F1()
        {
            for (int i = 0; i < 42; i++) { }  // Noncompliant

            switch (a) { /* This commit doesn't count */ } // Noncompliant

            try { } // Noncompliant
            catch (Exception e)
            {
                // Ignore
            }
            catch { } // Noncompliant
            finally { } // Noncompliant

            for (int i = 0; i < 42; i++) ;
            for (int i = 0; i < 42; i++) { Console.WriteLine(i); }

            if (a) { /* Do nothing because of X and Y */ }

            switch (a)
            {
                case 1:
                    break;
                default:
                    break;
            }

            unsafe { } // Noncompliant

            var foo1 = x => { }
            var foo2 = (x) => { }
            var foo3 = delegate { } 
        }

        void F2()
        {
        }
    }

    public class EmptyClass
    {
    }
}
