using System;

namespace Tests.Diagnostics
{
    public class IfConditionalAlwaysTrueOrFalse
    {
        public void DoSomething() { throw new NotSupportedException(); }
        public void DoSomething2() { throw new NotSupportedException(); }
        public void DoSomethingArgs(bool a, bool b,int i)
        {
            var someWronglyFormatted = 45;
            if (a == b)
            {
                DoSomething();
            }

            if (true == b)
            {
                DoSomething2();
            }

            if (a)
            {
                DoSomething();
            }

            if (true) // false negative, not detected by CBDE
            {
                DoSomething2();
            }

            if (false) // false negative, not detected by CBDE
            {
                DoSomething();
            }

            if (true) // false negative, not detected by CBDE
            {
                DoSomething();
            }
            else // false negative, not detected by CBDE
            {
            }

            if (false) // false negative, not detected by CBDE
            {
                DoSomething2();
            }
            else
            {
                ; ;
            }

            int j = 0;
            if (j == 0) // Noncompliant {{Condition is always true}}
            {
                DoSomething2();
            }
            else
            {
                ; ;
            }

            if (i == 0)
            {
                if (i == 0) // Noncompliant {{Condition is always true}}
                {
                    DoSomething2();
                }
            }

            j = i;
            if (i == 0)
            {
                if (j == 0) // Noncompliant {{Condition is always true}}
                {
                    DoSomething2();
                }
            }
        }
    }
}
