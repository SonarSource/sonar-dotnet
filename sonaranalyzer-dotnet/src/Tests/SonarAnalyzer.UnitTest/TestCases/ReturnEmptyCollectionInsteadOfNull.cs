using System;
using System.Collections.Generic;

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

        IEnumerable<int> GetValues(string str)
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

        int Age { get; private set; }
    }
}
