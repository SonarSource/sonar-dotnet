using System.Diagnostics.CodeAnalysis;

namespace CSharpLatest.CSharp11Features;

internal class RequiredMembers
{
    public class Person
    {
        public Person() { }

        [SetsRequiredMembers]
        public Person(string firstName) => FirstName = firstName;

        public required string FirstName { get; init; }
    }

    public void Method()
    {
        var person = new Person("John");
        person = new Person { FirstName = "John" };
    }
}
