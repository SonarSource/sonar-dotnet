using System;

namespace Tests.Diagnostics
{
    public class IfConditionalAlwaysTrueOrFalse
    {
        public void DoSomething() { throw new NotSupportedException(); }
        public void DoSomething2() { throw new NotSupportedException(); }
        public IfConditionalAlwaysTrueOrFalse(bool a, bool b)
        {
            var someWronglyFormatted =      45     ;
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

            if (true) // Noncompliant {{Remove this useless 'if' statement.}}
//          ^^^^^^^^^
            {
                DoSomething2();
            }

            if (false) // Noncompliant {{Remove this useless 'if' statement.}}
            {
                DoSomething();
            }

            if (true) // Noncompliant
            {
                DoSomething();
            }
            else // Noncompliant {{Remove this useless 'else' clause.}}
            {
            }

            if (false) // Noncompliant
            {
                DoSomething2();
            }
            else
            {
                ; ;
            }
        }
    }
}
