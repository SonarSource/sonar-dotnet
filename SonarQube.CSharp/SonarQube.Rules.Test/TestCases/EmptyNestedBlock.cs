using System;

namespace Tests.Diagnostics
{
    public class EmptyNestedBlock
    {
        public EmptyNestedBlock()
        {
        }

        ~EmptyNestedBlock()
        {
        }

        void F1(bool b)
        {
            for (int i = 0; i < 42; i++) { }  // Noncompliant
            var a = b;
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
                case true:
                    break;
                default:
                    break;
            }

            unsafe { } // Noncompliant

            var foo1 = new Action<object>(x => { });
            var foo2 = new Action<object>((x) => { });
            var foo3 = new Action(delegate { });
        }

        void F2()
        {
        }
    }

    public class EmptyClass
    {
    }
}
