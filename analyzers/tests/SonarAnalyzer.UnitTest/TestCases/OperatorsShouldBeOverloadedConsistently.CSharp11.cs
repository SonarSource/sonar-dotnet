public class Foo : IEquality<Foo> // Noncompliant FP Repro for: https://github.com/SonarSource/sonar-dotnet/issues/6442
{
    public static Foo operator +(Foo a, Foo b) => new Foo();

    public override bool Equals(object obj) => false;
    public override int GetHashCode() => 0;
}

public interface IEquality<TSelf> where TSelf : IEquality<TSelf>
{
    static virtual bool operator ==(TSelf a, TSelf b) => true;
    static virtual bool operator !=(TSelf a, TSelf b) => false;
}

// Issues are not raised in Interface implemetnations.
// Interfaces are used to describe the operators supported by a type in a fine grained way
public interface IPlus<TSelf> where TSelf : IPlus<TSelf>
{
    static virtual TSelf operator +(TSelf a, TSelf b) => a;
}
public interface IMinus<TSelf> where TSelf : IMinus<TSelf>
{
    static abstract TSelf operator -(TSelf a, TSelf b);
}
public interface IMulti
{
    static IMulti operator *(IMulti a, IMulti b) => a;
}
