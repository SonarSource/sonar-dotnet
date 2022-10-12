using System.Diagnostics.CodeAnalysis;

namespace Net7.features
{
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
}
