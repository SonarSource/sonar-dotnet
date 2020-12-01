using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using var ms = new MemoryStream();
new BinaryFormatter().Deserialize(ms); // FN

var topLevel = new BinaryFormatter();
topLevel.Binder = new SafeBinder();
topLevel.Deserialize(ms); // Compliant: safe binder was used

void TopLevelLocalFunction(MemoryStream ms)
{
    new BinaryFormatter().Deserialize(ms); // FN

    var local = new BinaryFormatter();
    local.Binder = new SafeBinder();
    local.Deserialize(ms); // Compliant: safe binder was used
}

public class Sample
{
    private string field;

    public void TargetTypedNew(MemoryStream ms)
    {
        BinaryFormatter formatter = new();
        formatter.Deserialize(ms); // FN

        formatter = new();
        formatter.Binder = new SafeBinder();
        formatter.Deserialize(ms); // Compliant: safe binder was used

        new BinaryFormatter().Deserialize(ms); // FN , can't build CFG for this method
    }

    public void PatternMatching(MemoryStream ms)
    {
        var formatter = new BinaryFormatter();
        formatter.Binder = new SafeBinderWithPatternMatching();
        formatter.Deserialize(ms); // Compliant: safe binder was used
    }

    public void StaticLambda()
    {
        Action<MemoryStream> a = static (ms) =>
        {
            new BinaryFormatter().Deserialize(ms);    // Noncompliant
        };
        a(null);
    }

    public int Property
    {
        get => 42;
        init
        {
            new BinaryFormatter().Deserialize(null);    // FN
        }
    }
}

public record Record
{
    public void Method(MemoryStream ms)
    {
        new BinaryFormatter().Deserialize(ms);    // Noncompliant
    }
}

public partial class Partial
{
    public partial void Method(MemoryStream ms);
}

public partial class Partial
{
    public partial void Method(MemoryStream ms)
    {
        new BinaryFormatter().Deserialize(ms);      // Noncompliant

        var formatter = new BinaryFormatter();
        formatter.Binder = new SafeBinderPartial();
        formatter.Deserialize(ms);                  // Noncompliant FP: safe binder was used
    }
}

internal sealed class SafeBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName) =>
        typeName == "TypeT" ? typeof(TypeT) : null;
}

internal sealed class SafeBinderWithPatternMatching : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName) =>
        typeName is string and "TypeT" ? typeof(TypeT) : null;
}

internal sealed partial class SafeBinderPartial : SerializationBinder
{
    public override partial Type BindToType(string assemblyName, string typeName);  // Secondary FP
}

internal sealed partial class SafeBinderPartial : SerializationBinder
{
    public override partial Type BindToType(string assemblyName, string typeName) =>
        typeName == "TypeT" ? typeof(TypeT) : null;
}

public class TypeT { }
