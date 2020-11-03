using System;

public record Person
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
        text ??= "empty"; // Noncompliant [3]
        person = new(); // Noncompliant [4]
    }

    public Person() : this(DateTime.Now)
    {
        var tmp = dateOfBirth.ToString(); // Compliant
    }
}
