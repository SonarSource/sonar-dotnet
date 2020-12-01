public record Compliant
{
    private readonly int x;

    public override int GetHashCode() => x.GetHashCode();
}

public record Noncompliant
{
    private readonly int x;

    public override int GetHashCode() => x.GetHashCode() ^ base.GetHashCode(); //Noncompliant - FP, this rule should not apply for records since Equals and GetHashCode are value-based
}
