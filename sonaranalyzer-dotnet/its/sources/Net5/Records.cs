using System;

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

    public abstract record AbstractRecord
    {
        public abstract void AbstractMethod();
    }

    public record PositionalConstructorRecord : AbstractRecord
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }

        public PositionalConstructorRecord(string firstName, string lastName)
          => (FirstName, LastName) = (firstName, lastName);

        public void Deconstruct(out string firstName, out string lastName)
          => (firstName, lastName) = (FirstName, LastName);

        public bool Method() => true;

        public virtual bool VirtualMethod() => true;

        public override void AbstractMethod() { }
    }

    public record InheritFromPositionalConstructorRecod : PositionalConstructorRecord
    {
        public InheritFromPositionalConstructorRecod(string firstName, string lastName) : base(firstName, lastName)
        {
        }

        public override bool VirtualMethod() => false;
    }

    public sealed record SealedRecord { public string Name; }

    public record FieldAndProperty
    {
        private int field = 42;
        public int Property => field;
    }

    public record Finalizer
    {
        ~Finalizer()
        {
        }
    }

    public record OperatorOverload
    {
        public static OperatorOverload operator +(OperatorOverload a) => a;
    }

    public record HashCodeRecord
    {
        private readonly int x;
        private int y;
        public HashCodeRecord(int x)
        {
            this.x = x;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ base.GetHashCode();
        }
    }

    public record DisposableRecord : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public record W<T>
    {
    }

    public record X<T> where T: class
    {
        public readonly T Property;
    }

    public record Y<T> where T : struct
    {
        public readonly T Property;
        private void Method<T>(T value)
        {
            if (value == null)
            {
                // ...
            }
        }
    }

    public record Z<T> : X<Z<Z<T>>>
    {
    }
}
