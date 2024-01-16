using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    class NotCompliantCases
    {
        private string[] stringArray = new string[10];

        public string[] Property1
        {
            get { return (string[])stringArray.Clone(); }
//                                 ^^^^^^^^^^^^^^^^^ Noncompliant {{Refactor 'Property1' into a method, properties should not copy collections.}}
        }

        public IEnumerable<string> Property2
        {
            get { return stringArray.ToArray(); }
//                       ^^^^^^^^^^^^^^^^^^^ Noncompliant {{Refactor 'Property2' into a method, properties should not copy collections.}}
        }

        public IEnumerable<string> Property3
        {
            get { return stringArray.ToList(); }
//                       ^^^^^^^^^^^^^^^^^^ Noncompliant {{Refactor 'Property3' into a method, properties should not copy collections.}}
        }

        public IEnumerable<string> Property4 => stringArray.ToList();
//                                              ^^^^^^^^^^^^^^^^^^ Noncompliant {{Refactor 'Property4' into a method, properties should not copy collections.}}

        public IEnumerable<string> Property5 => stringArray.Where(s => s != null).ToList();
//                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Refactor 'Property5' into a method, properties should not copy collections.}}

        public string[] Property6 => (string[])stringArray.Clone(); // Noncompliant

        public string[] Property7
        {
            get => stringArray.ToArray(); // Noncompliant
        }
    }

    class CompliantCases
    {
        private static string[] staticStrings = new string[] { "a", "b", "c" };

        private string[] stringArray;

        public string[] LazyInitialization1
        {
            get { return stringArray ?? (stringArray = (string[])staticStrings.Clone()); }
        }

        public string[] LazyInitialization2
        {
            get
            {
                var value = staticStrings.ToArray();
                return value;
            }
        }

        public string[] CloneInSetter
        {
            get { return null; }
            set { stringArray = (string[])staticStrings.Clone(); }
        }

        public string[] CloneInMethod()
        {
            return (string[])stringArray.Clone();
        }
    }
}
