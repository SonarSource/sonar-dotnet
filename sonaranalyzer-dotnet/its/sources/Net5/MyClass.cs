using System;
using System.Collections;

// needed due to a bug - see https://github.com/dotnet/roslyn/issues/45510 , it should get fixed in Preview 7
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}

namespace Net5
{
    public class MyClass
    {
        public record PublicPerson { public string FirstName; public string LastName; }

        public static void Foo()
        {
            Console.WriteLine("I will print the persons in Foo()");
            var person = new PrivatePerson("Scott", "Hunter"); // positional construction
            var otherPerson = person with { LastName = "Hanselman" };
            if (person.FirstName == otherPerson.LastName)
            {
                Console.WriteLine("Impossible");
                Console.WriteLine(true == true);
            }
            Console.WriteLine($"{person.FirstName} + {person.LastName}");
            Console.WriteLine($"{otherPerson.FirstName} + {otherPerson.LastName}");
        }

        private record PrivatePerson
        {
            public string FirstName { get; init; }
            public string LastName { get; init; }
            public PrivatePerson(string firstName, string lastName)
              => (FirstName, LastName) = (firstName, lastName);
            public void Deconstruct(out string firstName, out string lastName)
              => (firstName, lastName) = (FirstName, LastName);
        }
    }
}
