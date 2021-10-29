using System;

public struct Person
{
    private static DateTime dateOfBirth;
//                          ^^^^^^^^^^^ Secondary [0]
    private static int expectedFingers; // Secondary [1]
    private static Person instance; // Secondary [2]
    private static string text; // Secondary [3]
    private static Person person; // Secondary [4]

    public Person(DateTime birthday)
    {
        dateOfBirth = birthday;  // Noncompliant [0] {{Remove this assignment of 'dateOfBirth' or initialize it statically.}}
//      ^^^^^^^^^^^^^
        expectedFingers = 10;  // Noncompliant [1] {{Remove this assignment of 'expectedFingers' or initialize it statically.}}
//      ^^^^^^^^^^^^^^^^^
        instance = this; // Noncompliant [2]
    }

    public Person() : this(DateTime.Now)
    {
        var tmp = dateOfBirth.ToString(); // Compliant
        text ??= "empty"; // Noncompliant [3]
        person = new(); // Noncompliant [4]
        (dateOfBirth, var y) = (DateTime.Now, "42"); // FN
    }
}

public record struct RecordStructPerson
{
    private static DateTime dateOfBirth;
//                          ^^^^^^^^^^^ Secondary [5]
    private static int expectedFingers; // Secondary [6]
    private static RecordStructPerson instance; // Secondary [7]
    private static string text; // Secondary [8]
    private static RecordStructPerson person; // Secondary [9]

    public RecordStructPerson(DateTime birthday)
    {
        dateOfBirth = birthday;  // Noncompliant [5] {{Remove this assignment of 'dateOfBirth' or initialize it statically.}}
//      ^^^^^^^^^^^^^
        expectedFingers = 10;  // Noncompliant [6] {{Remove this assignment of 'expectedFingers' or initialize it statically.}}
//      ^^^^^^^^^^^^^^^^^
        instance = this; // Noncompliant [7]
    }

    public RecordStructPerson() : this(DateTime.Now)
    {
        var tmp = dateOfBirth.ToString(); // Compliant
        text ??= "empty"; // Noncompliant [8]
        person = new(); // Noncompliant [9]
        (dateOfBirth, var y) = (DateTime.Now, "42"); // FN
    }
}
