using System;

namespace Tests.Diagnostics
{
    public class IfConditionalAlwaysTrueOrFalse
    {
        public void DoSomething(  ) { throw new NotSupportedException(); }
        public void DoSomething2(  ) { throw new NotSupportedException(); }
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
            DoSomething2();
            DoSomething();
            ; ;
        }
    }
}
