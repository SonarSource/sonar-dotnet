public record Compliant
{
    private readonly int x;

    public override int GetHashCode() => x.GetHashCode();
}

public record BaseGetHashCodeUsed
{
    private readonly int x;

    public override int GetHashCode() => x.GetHashCode() ^ base.GetHashCode(); // Althoug probably worng this is compliant with current rule since GetHashCode is not using references.
}

public record WithParameters(string X)
{
    public override int GetHashCode() => X.GetHashCode();
}
