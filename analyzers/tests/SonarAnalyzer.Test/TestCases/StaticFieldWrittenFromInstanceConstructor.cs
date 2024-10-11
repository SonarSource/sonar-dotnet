using System;

namespace SonarAnalyzer.Test.TestCases
{
    public class Person
    {
        private static DateTime dateOfBirth;
//                              ^^^^^^^^^^^ Secondary [0]
        private static int expectedFingers; // Secondary [1]
        private static Person instance; // Secondary [2]
        private static string text; // Secondary

        public Person(DateTime birthday)
        {
            dateOfBirth = birthday;  // Noncompliant [0] {{Remove this assignment of 'dateOfBirth' or initialize it statically.}}
//          ^^^^^^^^^^^^^
            expectedFingers = 10;  // Noncompliant [1] {{Remove this assignment of 'expectedFingers' or initialize it statically.}}
//          ^^^^^^^^^^^^^^^^^
            instance = this; // Noncompliant [2]

            text ??= "empty"; // Noncompliant
        }

        public Person() : this(DateTime.Now)
        {
            var tmp = dateOfBirth.ToString(); // Compliant
        }

        static Person()
        {
            dateOfBirth = DateTime.Now; // Compliant
        }
    }

    public partial class PartialPerson
    {
        private static DateTime dateOfBirth; // Secondary
    }

    public partial class PartialPerson
    {
        public PartialPerson(DateTime birthday)
        {
            dateOfBirth = birthday;  // Noncompliant; now everyone has this birthday
        }
    }
}
