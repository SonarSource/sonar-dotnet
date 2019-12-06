using System;

namespace Tests.Diagnostics
{
    public class IfConditionalAlwaysTrueOrFalse
    {
        public void PositiveOverflow() {
            int i = 2147483600;
            i +=100; // Noncompliant {{This operation always overflows}}
        }
        public void NegativeOverflow() {
            int i = -2147483600;
            i -=100; // Noncompliant {{This operation always overflows}}
        }
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

            if (true) // always true (but rule not actvated)
            {
                DoSomething2();
            }

            if (false) // always false (but rule not actvated)
            {
                DoSomething();
            }

            if (true) // always true (but rule not actvated)
            {
                DoSomething();
            }
            else // false negative, not detected by CBDE
            {
            }

            if (false) // always false (but rule not actvated)
            {
                DoSomething2();
            }
            else
            {
                ; ;
            }

            int j = 0;
            if (j == 0) // always true (but rule not actvated)
            {
                DoSomething2();
            }
            else
            {
                ; ;
            }

            if (i == 0)
            {
                if (i == 0) // always true (but rule not actvated)
                {
                    DoSomething2();
                }
            }

            j = i;
            if (i == 0)
            {
                if (j == 0) // always true (but rule not actvated)
                {
                    DoSomething2();
                }
            }
        }
    }
}
