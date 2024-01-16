using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

[assembly: System.Security.AllowPartiallyTrustedCallers()]

[Serializable]
public record Foo : ISerializable
{
    private int n;

    [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
    public Foo()
    {
        n = -1;
    }

    protected Foo(SerializationInfo info, StreamingContext context) // Noncompliant
    {
        n = (int)info.GetValue("n", typeof(int));
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("n", n);
    }
}

[Serializable]
public record Foo_ok : ISerializable
{
    [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
    public Foo_ok() { }

    [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
    protected Foo_ok(SerializationInfo info, StreamingContext context) { } // Compliant

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
}

[Serializable]
public record Bar : ISerializable
{
    [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
    [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
    public Bar() { }

    [GacIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
    public Bar(int i) { }

    [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
    [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
    protected Bar(SerializationInfo info, StreamingContext context) { } // Noncompliant

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
}
