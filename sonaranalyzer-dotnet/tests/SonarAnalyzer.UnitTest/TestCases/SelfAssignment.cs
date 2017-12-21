using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class SelfAssignment
    {
        public SelfAssignment()
        {

        }
        public SelfAssignment(int Prop1)
        {

        }
        public int Prop1 { get; set; }
        public void Test()
        {
            var Prop1 = 5;
            Prop1 = Prop1; //Noncompliant
//          ^^^^^^^^^^^^^

            Prop1 = 2*Prop1;

            var y = 5;
            y = /*comment*/ y; //Noncompliant {{Remove or correct this useless self-assignment.}}

            var x = new SelfAssignment
            {
                Prop1 = Prop1
            };
            x = new SelfAssignment(Prop1: Prop1);
            var z = new
            {
                Prop1 = Prop1
            };
        }
    }
}
