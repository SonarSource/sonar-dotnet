using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

var list = new List<int>();
list.Any(x => x > 0);       // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
list?.Any(x => x > 0);      // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
list.Any(x => x == 0);      // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
list?.Any(x => x == 0);     // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
list.Append(1).Any(x => x > 0);
list.Any();
list.Exists(x => x > 0);

var dog = new Dog();
var pets = new List<Pet> { dog };
pets.Any(x => x.Equals(dog));       // Noncompliant FP NET-4998
pets.Contains(dog);
var petSet = new HashSet<Pet> { dog };
petSet.Any(x => x.Equals(dog));     // Noncompliant FP NET-4998
decimal amount = 5m;
var moneys = new List<Money> { amount };
moneys.Any(x => x.Equals(amount));  // Noncompliant FP NET-4998
int number = 1;
list.Any(x => x.Equals(number));    // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}

var immutableList = ImmutableList.Create<int>();
immutableList.Any(x => x > 0);  // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList?.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Append(1).Any(x => x > 0);
immutableList.Any();
immutableList.Exists(x => x > 0);

int someInt = 1;
immutableList.Any(x => x == 0);         // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => 0 == x);         // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => x == someInt);   // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => someInt == x);   // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => x.Equals(0));    // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => 0.Equals(x));        // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => x.Equals(x + 1));    // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

public class Sample
{
    public List<int> FieldKeyword
    {
        get;
        set
        {
            if (field.Any(x => x == 0)) // Noncompliant
            {
                field.AddRange(value);
            }
        }
    }
}

public class Cat;
public class Dog;
public union Pet(Cat, Dog);

public struct Money
{
    public decimal Value;
    public static implicit operator Money(decimal d) => new Money { Value = d };
}
