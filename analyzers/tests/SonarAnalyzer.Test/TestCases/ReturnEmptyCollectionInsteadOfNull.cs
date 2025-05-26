using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Tests.Diagnostics
{
    class Program
    {
        IEnumerable<string> ArrowedStrings1 => null; // Noncompliant {{Return an empty collection instead of null.}}
//                                             ^^^^

        IEnumerable<string> ArrowedStrings2 => (null); // Noncompliant

        IEnumerable<string> Strings2
        {
            get
            {
                return null; // Noncompliant
//                     ^^^^
            }
        }

        IEnumerable<string> Strings3
        {
            get
            {
                return ((null)); // Noncompliant
            }
        }

        IEnumerable<string> PropertyNoGetter { set { } }

        IEnumerable<string> ArrowedGetStrings1() => null; // Noncompliant
//                                                  ^^^^
        IList<string> ArrowedGetStrings2 => null; // Noncompliant
        string[] ArrowedGetStrings3() => null; // Noncompliant
        ICollection<string> ArrowedGetStrings4() => null; // Noncompliant
        Array ArrowedGetArray() => null; // Noncompliant
        object ArrowedGetObject() => null; // Compliant

        IEnumerable<char> GetValues(string str)
        {
            if (str == null)
            {
                return null; // Noncompliant
//                     ^^^^
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

        // See https://github.com/SonarSource/sonar-dotnet/issues/761
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

        static IEnumerable<string> TestNullFromLambda()
        {
            var list = new List<string>();

            return list.Select(o =>
            {
                if (o != null)
                {
                    return o;
                }

                return null;
            });
        }

        static IEnumerable<string> TestNullFromParenthesizedLambda()
        {
            var list = new List<string>();

            return list.Select<string, string>((o, i) =>
            {
                return null;
            });
        }

        static IEnumerable<string> MethodWithLambdaStillRaisesIssue()
        {
            var list = new List<string>();

            return list.Select<string, string>((o, i) =>
            {
                return null;
            });
            return null; // Noncompliant
        }

        List<string> NoncompliantMethodWithSecondaryLocation(string str)
        {
            switch (str)
            {
                case "First":
                    return null; // Noncompliant
                case "Second":
                    return new List<string> { };
                default:
                    return null; // Secondary {{Return an empty collection instead of null.}}
            }
        }
    }
}

class PropertyWithoutAccessorList
{
    IEnumerable<string> Incomplete => ; // Error [CS1525]: Invalid expression term
}

// Reproducer for #6494 https://github.com/SonarSource/sonar-dotnet/issues/6494
namespace Issue_6494
{
    class MyClass
    {
        public IEnumerable<string> this[int i] => null; // Noncompliant
        public static implicit operator List<int>(MyClass c) => null; // Noncompliant
    }
    class MyOtherClass
    {
        public IEnumerable<string> this[int index]
        {
            get
            {
                return null; // Noncompliant
            }
        }
        public static implicit operator List<int>(MyOtherClass c)
        {
            return null; // Noncompliant
        }
    }
}
