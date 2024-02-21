using System;

public record AnyOther
{
    public int Field;
}

public record GetHashCodeMutable : AnyOther
{
    public readonly DateTime birthday;
    public const int Zero = 0;
    public int age;
    public string name;
    int foo, bar;

    public override int GetHashCode() // Noncompliant {{Refactor 'GetHashCode' to not reference mutable fields.}}
    {
        int hash = Zero;
        hash += foo.GetHashCode(); // Secondary {{Remove this use of 'foo' or make it 'readonly'.}}
//              ^^^
        hash += age.GetHashCode(); // Secondary {{Remove this use of 'age' or make it 'readonly'.}}
        hash += this.name.GetHashCode(); // Secondary {{Remove this use of 'name' or make it 'readonly'.}}
        hash += name.GetHashCode(); // Compliant, we already reported on this symbol
        hash += this.birthday.GetHashCode();
        hash += SomeMethod(Field); // Secondary {{Remove this use of 'Field' or make it 'readonly'.}}
        return hash;
    }

    public int SomeMethod(int value)
    {
        int hash = Zero;
        hash += this.age.GetHashCode();
        return hash;
    }
}

public struct Struct
{
    public int Field;

    // See https://github.com/SonarSource/sonar-dotnet/issues/8756
    public override int GetHashCode() => Field; // Compliant, this is a value type.
}

public record struct RecordStruct
{
    public int Field;

    // See https://github.com/SonarSource/sonar-dotnet/issues/8756
    public override int GetHashCode() => Field; // Compliant, this is a value type.
}

