Apple apple = new();
Fruit f = apple;

var b = f is Apple;     // Compliant
b = f is not Apple;     // Compliant
b = apple.GetType() == typeof(int?);                // Compliant
b = apple.GetType().IsInstanceOfType(f.GetType());  // Compliant
b = f as Apple == null;         // Noncompliant

Pet pet = new UnionDog();
b = typeof(UnionDog).IsInstanceOfType(pet);             // Compliant NET-4991: 'is' matches the union's Value, reflection sees the union struct
b = typeof(UnionDog).IsAssignableFrom(pet.GetType());   // Compliant NET-4991
b = pet.GetType() == typeof(UnionDog);                  // Compliant NET-4991
b = pet.GetType() != typeof(UnionDog);                  // Compliant NET-4991
b = pet is UnionDog;                                    // Compliant
b = typeof(Pet).IsInstanceOfType(pet);                  // Compliant, 'pet is Pet' does not compile
b = pet.GetType() == typeof(Pet);                       // Compliant
Pet? nullablePet = pet;
b = typeof(UnionDog).IsInstanceOfType(nullablePet);     // Compliant, 'is' matches the Value through Nullable<T> too
b = nullablePet.GetType() == typeof(UnionDog);          // Compliant
object boxedPet = pet;
b = typeof(UnionDog).IsInstanceOfType(boxedPet);        // Noncompliant, 'boxedPet is UnionDog' is false as well
b = boxedPet.GetType() == typeof(UnionDog);             // Noncompliant
System.Type dogType = typeof(UnionDog);
b = dogType.IsAssignableFrom(pet.GetType());            // Noncompliant, 'IsInstanceOfType' sees the union struct as well
b = typeof(UnionDog).IsInstanceOfType(null);            // Noncompliant, an argument without a type
b = typeof(UnionDog) == pet.GetType();                  // Compliant NET-4991, reversed operands
b = typeof(UnionDog) == boxedPet.GetType();             // Noncompliant
Result<int> result = new UnionDog();
b = typeof(UnionDog).IsInstanceOfType(result);          // Compliant NET-4991, a generic union
b = result.GetType() == typeof(UnionDog);               // Compliant NET-4991
CustomPet customPet = new CustomPet(new UnionDog());
b = typeof(UnionDog).IsInstanceOfType(customPet);              // Compliant NET-4991, a custom [Union] type behaves like a union declaration
b = typeof(UnionDog).IsAssignableFrom(customPet.GetType());    // Compliant NET-4991
b = customPet.GetType() == typeof(UnionDog);                   // Compliant NET-4991
ClassPet classPet = new ClassPet(new UnionDog());
b = typeof(UnionDog).IsInstanceOfType(classPet);               // Compliant NET-4991, a class can be a custom union too
b = classPet.GetType() == typeof(UnionDog);                    // Compliant NET-4991
PetLookalike lookalike = new PetLookalike(new UnionDog());
b = typeof(PetLookalike).IsInstanceOfType(lookalike);          // Noncompliant, same shape without [Union] is not a union
b = lookalike.GetType() == typeof(PetLookalike);               // Noncompliant
b = f as Apple is null;         // Noncompliant
b = f as Apple is not null;     // Noncompliant
b = f as Apple is not not not not null; // Noncompliant
b = f as Apple is Apple { Size: 42 };   // Compliant
b = f as Apple is Apple;        // Noncompliant
b = true && (apple) is Apple;   // Noncompliant
b = !(apple is Apple);          // Noncompliant

if (apple is { }) { }           // Compliant

b = apple is ("Sweet", "Red");
b = apple is { Taste: "Sweet", Color: "Red" };

b = f as Apple is Apple { Prop.Length: 42 };   // Compliant
b = f as Apple == null;         // Noncompliant

record Fruit
{
    public int Size { get; }
    public int[] Prop { get; }
}

sealed record Apple : Fruit
{
    public string Taste;
    public string Color;
    public void Deconstruct(out string x, out string y) => (x, y) = (Taste, Color);
}

public class SomeClass : System.Collections.ArrayList
{
    public bool SomeMethod(SomeClass a)
    {
        return a is [SomeClass];
    }
}

public class UnionCat;
public sealed class UnionDog;
public union Pet(UnionCat, UnionDog)
{
    public bool IsDog() => GetType() == typeof(UnionDog);               // Compliant NET-4991, implicit 'this' receiver
    public bool IsDogQualified() => this.GetType() == typeof(UnionDog); // Compliant NET-4991
    public bool IsDogAssignable() => typeof(UnionDog).IsAssignableFrom(GetType()); // Compliant NET-4991
    public bool IsDogArgument() => typeof(UnionDog).IsInstanceOfType(this);        // Compliant NET-4991

    public class Nested
    {
        public bool IsDog() => GetType() == typeof(UnionDog); // Noncompliant, the nested class is not a union
    }
}

[System.Runtime.CompilerServices.Union]
public struct CustomPet
{
    public CustomPet(UnionCat value) => Value = value;
    public CustomPet(UnionDog value) => Value = value;
    public object Value { get; }
    public bool IsDog() => GetType() == typeof(UnionDog);           // Compliant NET-4991
}

[System.Runtime.CompilerServices.Union]
public sealed class ClassPet : System.Runtime.CompilerServices.IUnion
{
    public ClassPet(UnionCat value) => Value = value;
    public ClassPet(UnionDog value) => Value = value;
    public object Value { get; }

    public bool IsDogInLambda()
    {
        System.Func<bool> isDog = () => GetType() == typeof(UnionDog); // Compliant NET-4991
        return isDog();
    }
}

public struct PetLookalike
{
    public PetLookalike(UnionCat value) => Value = value;
    public PetLookalike(UnionDog value) => Value = value;
    public object Value { get; }
    public bool IsSelf() => this.GetType() == typeof(PetLookalike); // Noncompliant
    public bool IsSelfImplicit() => GetType() == typeof(PetLookalike); // Noncompliant
    public bool IsSelfAssignable() => typeof(PetLookalike).IsAssignableFrom(GetType()); // Noncompliant
    public bool IsSelfArgument() => typeof(PetLookalike).IsInstanceOfType(this); // Noncompliant
}

public sealed class ClassLookalike
{
    public bool IsDogInLambda()
    {
        System.Func<bool> isDog = () => GetType() == typeof(UnionDog); // Noncompliant
        return isDog();
    }
}

public union Result<T>(T, UnionDog)
{
    public bool IsDog() => GetType() == typeof(UnionDog); // Compliant NET-4991
}

public struct GenericLookalike<T>
{
    public bool IsSelf() => GetType() == typeof(GenericLookalike<T>); // Noncompliant
    public static bool IsDog(T value) => typeof(UnionDog).IsInstanceOfType(value); // Noncompliant, a type parameter is not a union, even when instantiated with one
}
