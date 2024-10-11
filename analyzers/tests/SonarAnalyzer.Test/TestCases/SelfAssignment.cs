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
            int? value = null;

            value ??= value;
//          ^^^^^           Noncompliant
//                    ^^^^^ Secondary@-1
        }

        public int Prop1 { get; set; }

        public void Test()
        {
            var Prop1 = 5;
            Prop1 = Prop1;
//          ^^^^^         Noncompliant
//                  ^^^^^ Secondary@-1

            Prop1 = 2*Prop1;

            var y = 5;
            y = /*comment*/ y; // Noncompliant {{Remove or correct this useless self-assignment.}}
                               // Secondary@-1

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

    // Repro for https://github.com/SonarSource/sonar-dotnet/issues/9667
    public class Sample
    {
        public string First { get; set; }
        public string Second { get; set; }

        public Sample(string first)
        {
            this.First = first;    // Compliant
            Second = Second;       // Noncompliant
                                   // Secondary @-1
            this.Second = Second;  // False Negative
            Second = this.Second;  // False Negative
        }
    }

}
