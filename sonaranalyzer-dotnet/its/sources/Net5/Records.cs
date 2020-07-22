using System;

// needed for Records due to a bug - see https://github.com/dotnet/roslyn/issues/45510 , it should get fixed in Preview 7
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}

namespace Net5
{
    public class Records
    {
        public void TestRecords()
        {
            var x = new PropertyOnlyRecord { FirstName = "C#", LastName = "9" };
            var b = new RecordInheritance { ID = 1 };
            Console.WriteLine(x.FirstName + x.LastName + b.ID);

            ConstructorOnlyRecord y = new("1", "2");
            Console.WriteLine(y.A + y.B);

            Console.WriteLine("I will print the persons in Foo()");
            var person = new PositionalConstructorRecord("Scott", "Hunter"); // positional construction
            var otherPerson = person with { LastName = "Hanselman" };
            if (person.FirstName == otherPerson.LastName)
            {
                Console.WriteLine("Impossible");
                Console.WriteLine(true == true);
            }
            Console.WriteLine($"{person.FirstName} + {person.LastName}");
            Console.WriteLine($"{otherPerson.FirstName} + {otherPerson.LastName}");
        }
    }
    

    public record PropertyOnlyRecord { public string FirstName; public string LastName; }

    public record RecordInheritance : PropertyOnlyRecord { public int ID; }

    public record ConstructorOnlyRecord(string A, string B);

    public record PositionalConstructorRecord
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }

        public PositionalConstructorRecord(string firstName, string lastName)
          => (FirstName, LastName) = (firstName, lastName);

        public void Deconstruct(out string firstName, out string lastName)
          => (firstName, lastName) = (FirstName, LastName);
    }
}
