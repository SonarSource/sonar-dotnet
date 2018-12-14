using System;
using System.Collections.Generic;
using System.Xml;

namespace Tests.Diagnostics
{
    class Program
    {
        IEnumerable<string> ArrowedStrings1 => null; // Noncompliant {{Return an empty collection instead of null.}}
//                                             ^^^^
        IEnumerable<string> Strings2
        {
            get
            {
                return null; // Noncompliant
//              ^^^^^^^^^^^^
            }
        }

        IEnumerable<string> ArrowedGetStrings1() => null; // Noncompliant
//                                                  ^^^^
        IList<string> ArrowedGetStrings2 => null; // Noncompliant
        string[] ArrowedGetStrings3() => null; // Noncompliant
        ICollection<string> ArrowedGetStrings4() => null; // Noncompliant
        Array ArrowedGetArray() => null; // Noncompliant

        IEnumerable<char> GetValues(string str)
        {
            if (str == null)
            {
                return null; // Noncompliant
//              ^^^^^^^^^^^^
            }

            return str.ToCharArray();
        }

        IEnumerable<int> AssignedToNullVariable(int value)
        {
            List<int> myList = null;
            return myList; // Compliant - should not be
        }

        IEnumerable<int> AssignedToNullVariable2(int value)
        {
            List<int> myList = null;

            if (value == 42)
            {
                myList = new List<int>();
            }

            return myList; // Compliant - should not be (null on one path)
        }

        void DoSomething()
        {
        }

        string GetString()
        {
            return null; // Compliant - string is a collection but we allow null
        }

        public int Age { get; private set; }

        // See https://github.com/SonarSource/sonar-csharp/issues/761
        public List<int> Method()
        {
            Func<int?> aFunc = () =>
            {
                return null; // Compliant because we return from a func
            };
            return new List<int>();
        }

        public XmlNode GetXmlNode()
        {
            return null; // Compliant XmlNode and its descendants are ignored
        }

        public XmlDocument GetXmlDocument()
        {
            return null; // Compliant XmlNode and its descendants are ignored
        }

        public XmlElement GetXmlElement()
        {
            return null; // Compliant XmlNode and its descendants are ignored
        }

        public IEnumerable<int> SomeProp
        {
            get => null; // Noncompliant
        }

        public IEnumerable<string> GetSomeStrings()
        {
            var list = new List<string>();
            return list;

            int? LocalFunc()
            {
                return null;
            }
        }

        public IEnumerable<string> GetSomeStrings2()
        {
            var list = new List<string>();
            return list;

            IEnumerable<string> LocalFunc()
            {
                return null; // Noncompliant
            }
        }
    }
}
