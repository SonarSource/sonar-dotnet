using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalStructureSameCondition_Switch
    {
        public int SwitchExpression(int a)
        {
            return a switch
            {
                10 => a * 2,
                20 => a * 2,
                50 => a * 2,
            };
        }

    }
}

namespace CShar14
{
    class FieldKeyword
    {
        public int Name
        {
            get
            {
                switch (field)
                {
                    case 1:                 // Secondary
                        field = field + 1;
                        break;
                    case 20:                // Noncompliant
                                            // Secondary@-1
                        field = field + 1;
                        break;
                    case 50:                // Noncompliant
                        field = field + 1;
                        break;
                    case 42:                // Compliant
                        field = 42;
                        break;
                }
                return field;
            }
            set { }
        }
    }
}
