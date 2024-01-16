using System;

public record R
{
    public bool Equals(R other) => true; // Error [CS8872]
}

public sealed record Q
{
    public bool Equals(Q other) => true; // Compliant
}

public record RecordWithVirtualEquals : IEquatable<RecordWithVirtualEquals> // Compliant
{
    public virtual bool Equals(RecordWithVirtualEquals other) => true;
}

public abstract record RecordWithAbstractEquals : IEquatable<RecordWithAbstractEquals> // Compliant
{
    public abstract bool Equals(RecordWithAbstractEquals other);
}

public record R1(string x); // Compliant
public record R2(string x, string y) : R1(x); // Compliant
