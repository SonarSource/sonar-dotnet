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
