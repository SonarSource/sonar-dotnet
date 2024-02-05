using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalsWithSameCondition
    {
        public void doTheThing(object o)
        {
        }

        public int a { get; set; }
        public int b { get; set; }
        public int c { get; set; }
        public void Test()
        {
            if (a == b)
            {
                doTheThing(b);
            }
            if (a == b) // Noncompliant {{This condition was just checked on line 20.}}
//              ^^^^^^
            {
                doTheThing(b);
            }
            if (a == b) // Noncompliant
            {
                doTheThing(b);
            }
            if (a == c)
            {
                doTheThing(c);
                c = 5;
            }
            if (a == c) // Compliant, c might be updated in the previous if
            {
                c++;
            }
            if (a == c) // Compliant, c might be updated in the previous if
            {
                ++c;
            }
            if (a == c) // Compliant, c might be updated in the previous if
            {
                ++c;
            }

            if (a == b && a == c) { }
            if (a == b && a == c) { } // Noncompliant
            if (a == c && a == b) { } // Compliant, even when the semantics is the same as the previous one. Properties or invocations can have side effects on the result.
        }

        public void TestSw()
        {
            switch (a)
            {
                case 1:
                    break;
            }
            switch (a) // Noncompliant {{This condition was just checked on line 58.}}
            {
                case 2:
                    break;
            }
        }

        public void EquivalentChecksForNull(object o)
        {
            if (o == null) { }
            if (null == o) { }      // Noncompliant, same meaning
            if (!!(o == null)) { }  // Noncompliant, same meaning
            if (!(o != null)) { }   // Noncompliant, same meaning
            if (!(null != o)) { }   // Noncompliant, same meaning
        }
    }

    public class Coverage
    {
        private string name;

        public void AssignedToFieldWithSameName(string name)
        {
            if (name == name)
            {
                this.name = null;
            }
            if (name == name) { } // Noncompliant
        }

        public void Undefined(string sameAsUndefinedField)
        {
            if (sameAsUndefinedField == sameAsUndefinedField)
            {
                this.sameAsUndefinedField = null; // Error [CS1061]: 'Coverage' does not contain a definition for 'sameAsUndefinedField'
            }
            if (sameAsUndefinedField == sameAsUndefinedField) { } // Noncompliant
        }

        public void NameMismatch(string otherName)
        {
            if (otherName == otherName)
            {
                this.name = null;
            }
            if (otherName == otherName) { } // Noncompliant
        }
    }
}
