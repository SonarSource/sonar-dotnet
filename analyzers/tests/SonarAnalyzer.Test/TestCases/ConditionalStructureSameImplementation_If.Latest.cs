using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp14
{
    class FieldKeyword
    {
        public int Name
        {
            get
            {
                if(field is 1)
                {                       // Secondary
                    field = field + 1;
                }
                else if(field is 20)
                {                       // Noncompliant
                    field = field + 1;  // Secondary@-1
                }
                else if (field is 50)
                {                       // Noncompliant
                    field = field + 1;
                }
                else if(field is 42)    // Compliant
                {
                    field = 42;
                }
                return field;
            }
            set { }
        }
    }
}

namespace CSharp15
{
    class LabeledBreakAndContinue
    {
        public void If(int[] values)
        {
            outer: foreach (var value in values)
            {
                foreach (var other in values)
                {
                    if (value == other)
                    {                       // Secondary
                        DoSomething();
                        continue outer;
                    }
                    else if (value > other)
                    {
                        DoSomething();
                        continue;
                    }
                    else if (value < other)
                    {
                        DoSomething();
                        break outer;
                    }
                    else if (value == 0)
                    {                       // Noncompliant
                        DoSomething();
                        continue outer;
                    }
                }
            }
        }

        private void DoSomething() { }
    }
}
