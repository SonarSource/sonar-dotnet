public record Compliant
{
    private readonly int x;

    public override int GetHashCode() => x.GetHashCode();
}

public record Noncompliant
{
    private readonly int x;

    public override int GetHashCode() => x.GetHashCode() ^ base.GetHashCode();
}
