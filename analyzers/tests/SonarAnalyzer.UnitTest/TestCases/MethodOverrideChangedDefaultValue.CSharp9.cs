public interface IMyInterface
{
    void Read(int i, int j = 5);
    void Write(int i, int j = 5);
}

public record Base : IMyInterface
{
    public void Read(int i, int j = 5) { } // Compliant

    public virtual void Write(int i, int j = 0) { } // Noncompliant {{Use the default parameter value defined in the overridden method.}}
}

public partial record Derived : Base
{
    public override partial void Write(int i, int j = 42); // Noncompliant
}

public partial record Derived
{
    public override partial void Write(int i, int j = 1024) // Noncompliant
    {
    }
}
