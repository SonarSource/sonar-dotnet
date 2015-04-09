using System;

namespace Tests.Diagnostics
{
    public class IfConditionalAlwaysTrueOrFalse
    {
        public void DoSomething() { throw new NotSupportedException(); }
        public IfConditionalAlwaysTrueOrFalse(bool a, bool b)
        {
            if (a == b)
            {
                DoSomething();
            }

            if (true == b)
            {
                DoSomething();
            }

            if (a)
            {
                DoSomething();
            }

            if (true) // Noncompliant
            {
                DoSomething();
            }

            if (false) // Noncompliant
            {
                DoSomething();
            }
        }
    }
}
