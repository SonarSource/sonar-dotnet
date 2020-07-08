using System;
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
namespace net5
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var person = new Person("Scott", "Hunter"); // positional construction
            var otherPerson = person with { LastName = "Hanselman" };
            Console.WriteLine(person);
            Console.WriteLine(otherPerson);
        }

        public record Person
        {
            public string FirstName { get; init; }
            public string LastName { get; init; }
            public Person(string firstName, string lastName)
              => (FirstName, LastName) = (firstName, lastName);
            public void Deconstruct(out string firstName, out string lastName)
              => (firstName, lastName) = (FirstName, LastName);
        }

        public record Person2 { string FirstName; string LastName; }
    }
}
