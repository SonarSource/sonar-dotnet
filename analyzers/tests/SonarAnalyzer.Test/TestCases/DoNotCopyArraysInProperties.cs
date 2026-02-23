using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tests.Diagnostics
{
    class NotCompliantCases
    {
        private string[] stringArray = new string[10];
        private List<string> stringList = new List<string>();
        private HashSet<int> intSet = new HashSet<int>();

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

        public List<string> Property8
        {
            get { return new List<string>(stringArray); }
//                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Refactor 'Property8' into a method, properties should not copy collections.}}
        }

        public List<string> Property9 => new List<string>(stringList);
//                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Refactor 'Property9' into a method, properties should not copy collections.}}

        public HashSet<int> Property10
        {
            get => new HashSet<int>(intSet); // Noncompliant
        }

        public List<string> Property11 => new List<string>(stringArray.Where(s => s != null));
//                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Refactor 'Property11' into a method, properties should not copy collections.}}
    }

    class CompliantCases
    {
        private static string[] staticStrings = new string[] { "a", "b", "c" };

        private string[] stringArray;
        private List<string> stringList;

        public string[] LazyInitialization1
        {
            get { return stringArray ?? (stringArray = (string[])staticStrings.Clone()); } // Compliant
        }

        public string[] LazyInitialization2
        {
            get
            {
                var value = staticStrings.ToArray();
                return value;
            }
        }

        public List<string> LazyInitialization3
        {
            get { return stringList ?? (stringList = new List<string>(staticStrings)); } // Compliant
        }

        public List<string> CopyNotInReturn
        {
            get
            {
                var value = new List<string>(staticStrings); // FN
                return value;
            }
        }

        public string[] CloneInSetter
        {
            get { return null; }
            set { stringArray = (string[])staticStrings.Clone(); }
        }

        public List<string> NewCollectionInSetter
        {
            get { return null; }
            set { stringList = new List<string>(staticStrings); }
        }

        public string[] CloneInMethod()
        {
            return (string[])stringArray.Clone();
        }

        public List<string> NewCollectionInMethod()
        {
            return new List<string>(stringArray);
        }

        public List<string> NewCollectionFromLiteral => new List<string> { "a", "b", "c" };

        public List<string> NewCollectionWithCapacity => new List<string>(10);

        public Func<string[]> ReturningLambdaThatCopies => () => stringArray.ToArray();                             // Compliant

        public Func<int, string[]> ReturningLambdaWithParameterThatCopies => x => stringArray.ToArray();            // Compliant

        public Func<string[]> ReturningAnonymousMethodThatCopies => delegate { return stringArray.ToArray(); };     // Compliant

        public string[] ReturningFromLocalFunction
        {
            get
            {
                return Local();

                string[] Local() => stringArray.ToArray();                                                          // Compliant
            }
        }

        public ReadOnlyCollection<string> ReadOnlyCollectionWrapper => new ReadOnlyCollection<string>(stringArray); // Compliant ReadOnlyCollection is a wrapper, not a copy

        public Parent ParentWrapper => new Parent(stringArray);                                                     // Compliant Not a known copying collection
    }

    class Parent
    {
        private string[] children;
        public Parent(string[] children) => this.children = children;
    }
}
