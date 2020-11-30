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
            var a = b;

            for (int i = 0; i < 42; i++) { }  // Noncompliant
//                                       ^^^
            for (int i = 0; i < 42; i++) ;
            for (int i = 0; i < 42; i++) { Console.WriteLine(i); }

            try { } // Noncompliant {{Either remove or fill this block of code.}}
            catch (Exception e)
            {
                // Ignore
            }
            catch { } // Noncompliant
            finally { } // Noncompliant

            if (a) { /* Do nothing because of X and Y */ }
            if (a)
            {
                // TODO
            }
            if (a) { } // Noncompliant

            switch (a) { /* This commit doesn't count */ } // Noncompliant

            switch (a)
            {
                case true:
                    break;
                default:
                    break;
            }

            while (a) { } // Noncompliant

            while (a)
            {
                // FIXME
            }

            while (a)
            {
                a = false;
            }

            unsafe { } // Noncompliant

            var foo1 = new Action<object>(x => { });
            var foo2 = new Action<object>((x) => { });
            var foo3 = new Action(delegate { });

            if (a)
            {
                if (a)
                {
                    if (a) { } // Noncompliant
                    switch (a) { /* This commit doesn't count */ } // Noncompliant
                    while (a)
                    {
                        for (int i = 0; i < 42; i++) { }  // Noncompliant
                    }
                }
            }
        }

        void F2()
        {
        }
    }

    public class EmptyClass
    {
    }
}
