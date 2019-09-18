using System;

namespace Tests.Diagnostics
{
    public class IfConditionalAlwaysTrueOrFalse
    {
        public void DoSomething() { throw new NotSupportedException(); }
        public void DoSomething2() { throw new NotSupportedException(); }
        public void DoSomething3() {
            goto Label;
        Label:
            return;
        }
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

            if (true) // Noncompliant {{Condition is always true}}
            {
                DoSomething2();
            }

            if (false) // Noncompliant {{Condition is always false}}
            {
                DoSomething();
            }

            if (true) // Noncompliant {{Condition is always true}}
            {
                DoSomething();
            }
            else // false negative, not detected by CBDE
            {
            }

            if (false) // Noncompliant {{Condition is always false}}
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
