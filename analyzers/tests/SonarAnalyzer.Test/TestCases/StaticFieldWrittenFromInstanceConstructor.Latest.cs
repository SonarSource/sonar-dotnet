using System;

namespace CSharp9
{
    public record Person
    {
        private static DateTime dateOfBirth;
//                              ^^^^^^^^^^^ Secondary [0]
        private static int expectedFingers; // Secondary [1]
        private static Person instance; // Secondary [2]
        private static string text; // Secondary [3]
        private static Person person; // Secondary [4]

        public Person(DateTime birthday)
        {
            dateOfBirth = birthday;  // Noncompliant [0] {{Remove this assignment of 'dateOfBirth' or initialize it statically.}}
//          ^^^^^^^^^^^^^
            expectedFingers = 10;  // Noncompliant [1] {{Remove this assignment of 'expectedFingers' or initialize it statically.}}
//          ^^^^^^^^^^^^^^^^^
            instance = this; // Noncompliant [2]
            text ??= "empty"; // Noncompliant [3]
            person = new(); // Noncompliant [4]
        }

        public Person() : this(DateTime.Now)
        {
            var tmp = dateOfBirth.ToString(); // Compliant
        }
    }
}

namespace CSharp10
{
    public struct Person
    {
        private static DateTime dateOfBirth;
//                              ^^^^^^^^^^^    Secondary [5]
        private static int expectedFingers; // Secondary [6]
        private static Person instance;     // Secondary [7]
        private static string text;         // Secondary [8]
        private static Person person;       // Secondary [9]
        private static string postalCode;   // Secondary [tuple1]
        private static string city;         // Secondary [tuple2]

        public Person(DateTime birthday)
        {
            dateOfBirth = birthday;  // Noncompliant [5] {{Remove this assignment of 'dateOfBirth' or initialize it statically.}}
//          ^^^^^^^^^^^^^
            expectedFingers = 10;    // Noncompliant [6] {{Remove this assignment of 'expectedFingers' or initialize it statically.}}
//          ^^^^^^^^^^^^^^^^^
            instance = this;         // Noncompliant [7]
        }

        public Person() : this(DateTime.Now)
        {
            var tmp = dateOfBirth.ToString(); // Compliant
            text ??= "empty";                 // Noncompliant [8]
            person = new();                   // Noncompliant [9]
            (postalCode, city, var country, _) = ("10001", "New York", "USA", "Earth");
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant    [tuple1]
//                       ^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant@-1 [tuple2]
        }
    }

    public record struct RecordStructPerson
    {
        private static DateTime dateOfBirth;
//                              ^^^^^^^^^^^            Secondary [10, 15]
        private static int expectedFingers;         // Secondary [11]
        private static RecordStructPerson instance; // Secondary [12]
        private static string text;                 // Secondary [13]
        private static RecordStructPerson person;   // Secondary [14]

        public RecordStructPerson(DateTime birthday)
        {
            dateOfBirth = birthday; // Noncompliant [10] {{Remove this assignment of 'dateOfBirth' or initialize it statically.}}
//          ^^^^^^^^^^^^^
            expectedFingers = 10;   // Noncompliant [11] {{Remove this assignment of 'expectedFingers' or initialize it statically.}}
//          ^^^^^^^^^^^^^^^^^
            instance = this;        // Noncompliant [12]
        }

        public RecordStructPerson() : this(DateTime.Now)
        {
            var tmp = dateOfBirth.ToString();            // Compliant
            text ??= "empty";                            // Noncompliant [13]
            person = new();                              // Noncompliant [14]
            (dateOfBirth, var y) = (DateTime.Now, "42"); // Noncompliant [15]
        }
    }
}

namespace CSharp11
{
    public class Person
    {
        private static int expectedFingers; // Secondary

        public Person(DateTime birthday)
        {
            expectedFingers >>>= 5; // Noncompliant
        }
    }
}
