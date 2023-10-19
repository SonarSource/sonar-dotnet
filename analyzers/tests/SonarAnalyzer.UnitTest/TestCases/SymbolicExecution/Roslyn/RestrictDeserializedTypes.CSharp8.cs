using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Web.UI;

internal class Serializer
{
    void BinderAsVariable(Stream stream, bool condition)
    {
        var formatter = new BinaryFormatter();
        formatter.Binder ??= new SafeBinderExpressionWithNull();
        formatter.Deserialize(stream);                                                      // Noncompliant FP: engine doesn't know Binder is null

        formatter = new BinaryFormatter();
        formatter.Binder ??= new UnsafeBinder();
        formatter.Deserialize(stream);                                                      // Noncompliant [unsafeBinder1]: unsafe binder

        formatter = null;
        formatter ??= new BinaryFormatter();
        formatter.Binder = new SafeBinderExpressionWithNull();
        formatter.Deserialize(stream);                                                      // Compliant: safe binder

        formatter = null;
        formatter ??= new BinaryFormatter();
        formatter.Binder = new UnsafeBinder();
        formatter.Deserialize(stream);                                                      // Noncompliant [unsafeBinder2]: unsafe binder
    }

    private void LocalFunctions(Stream stream)
    {
        static UnsafeBinder LocalBinderFactoryUnsafe() => new UnsafeBinder();
        new BinaryFormatter { Binder = LocalBinderFactoryUnsafe() }.Deserialize(stream);    // Noncompliant [unsafeBinder3]: unsafe binder used

        static SafeBinderExpressionWithNull LocalBinderFactorySafe() => new SafeBinderExpressionWithNull();
        new BinaryFormatter { Binder = LocalBinderFactorySafe() }.Deserialize(stream);      // Compliant: safe binder used
    }

    internal void DeserializeOnExpression(MemoryStream memoryStream, bool condition)
    {
        BinaryFormatter bin = null;
        (bin ??= new BinaryFormatter()).Deserialize(memoryStream);                          // Noncompliant - unsafe in null coalescence
    }

    internal void Switch(Stream stream, bool condition, int number)
    {
        var formatter = new BinaryFormatter();
        formatter.Binder = condition switch {true => new UnsafeBinder(), false => null};
        formatter.Deserialize(stream);                                                      // Noncompliant [unsafeBinder4]: binder can be null or unsafe

        formatter = new BinaryFormatter();
        formatter.Binder = condition switch {true => new UnsafeBinder(), false => new UnsafeBinder()};
        formatter.Deserialize(stream);                                                      // Noncompliant [unsafeBinder5]

        formatter = new BinaryFormatter();
        formatter.Binder = condition switch {true => new SafeBinderStatementWithReturnNull(), false => new SafeBinderStatementWithReturnNull()};
        formatter.Deserialize(stream);
    }
}

internal sealed class SafeBinderStatementWithReturnNull : SerializationBinder
{
    public override Type BindToType(String assemblyName, System.String typeName)
    {
        if (typeName == "typeT")
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }

        return null;
    }
}

internal sealed class SafeBinderExpressionWithNull : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName) =>
        typeName == "typeT"
            ? Assembly.Load(assemblyName).GetType(typeName)
            : null;
}

internal sealed class UnsafeBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    //                   ^^^^^^^^^^ Secondary [unsafeBinder1, unsafeBinder2, unsafeBinder3, unsafeBinder4, unsafeBinder5] {{This method allows all types.}}
    {
        return Assembly.Load(assemblyName).GetType(typeName);
    }
}
