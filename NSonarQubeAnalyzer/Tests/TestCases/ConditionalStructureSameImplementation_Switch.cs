using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalStructureSameCondition
    {
        public int prop { get; set; }
        private void doSomething(int i) { }
        public void Test()
        {
            switch (i)
            {
                case 1:
                    doSomething(prop);
                    break;
                case 2:
                    doSomethingDifferent();
                    break;
                case 3:  // Noncompliant;
                    this.doSomething(this.prop);
                    break;
                case 4:
                    {
                        doSomething2();

                        break;
                    }
                case 5: // Noncompliant;
                    { //some comment here and there
                        doSomething2();
                        break;
                    }
                default: // Noncompliant;
                    doSomething(prop);
                    break;
            }

            switch (i)
            {
                case 1:
                case 3:
                    doSomething();
                    break;
                case 2:
                    doSomethingDifferent();
                    break;
                default:
                    doTheRest();
            }
        }
    }
}
