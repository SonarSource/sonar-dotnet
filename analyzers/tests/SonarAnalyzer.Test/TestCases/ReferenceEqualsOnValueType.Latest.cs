public record class Cat(string Name);
public record class Dog(string Name);
public union Pet(Cat, Dog);

class UnionComparison
{
    void Compare(Pet pet, Pet other)
    {
        var sameReference = object.ReferenceEquals(pet, other);    // Noncompliant
        var sameValue = pet.Equals(other);                         // Compliant
    }
}
