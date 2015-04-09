using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalStructureSameCondition_Switch
    {
        public int prop { get; set; }
        private void doTheRest() { throw new NotSupportedException(); }
        private void doSomething() { throw new NotSupportedException(); }
        private void doSomething(int i) { throw new NotSupportedException(); }
        private void doSomethingDifferent() { throw new NotSupportedException(); }
        private void doSomething2() { throw new NotSupportedException(); }
        public void Test()
        {
            var i = 5;
            switch (i)
            {
                case 1:
                    doSomething(prop);
                    break;
                case 2:
                    doSomethingDifferent();
                    break;
                case 3:  // Noncompliant;
                    doSomething(prop);
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
                    break;
            }
        }
    }
}
