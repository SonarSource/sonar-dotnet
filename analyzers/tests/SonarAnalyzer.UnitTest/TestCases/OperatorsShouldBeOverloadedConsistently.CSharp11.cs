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
