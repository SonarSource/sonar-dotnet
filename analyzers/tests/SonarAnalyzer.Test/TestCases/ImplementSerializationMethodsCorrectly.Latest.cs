using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

[OnSerialized] // Noncompliant {{Serialization attributes on local functions are not considered.}}
int OnSerialized(StreamingContext context) => 42;

public interface IWithValidSerializationMethods
{
    [OnSerializing]
    protected abstract void OnSerializingMethod(StreamingContext context);

    [OnSerialized]
    protected abstract void OnSerializedMethod(StreamingContext context);

    [OnDeserializing]
    protected void OnDeserializingMethod(StreamingContext context) { }

    [OnDeserialized]
    protected virtual void OnDeserializedMethod(StreamingContext context) { }
}

public interface IWithInvalidSerializationMethodsModifiers
{
    [OnSerializing]
    public abstract void OnSerializingMethod(StreamingContext context); // Compliant, despite "public", it is in interface

    [OnSerialized]
    public abstract void OnSerializedMethod(StreamingContext context);

    [OnDeserializing]
    public void OnDeserializingMethod(StreamingContext context) { }

    [OnDeserialized]
    public virtual void OnDeserializedMethod(StreamingContext context) { }
}

public interface IWithInvalidSerializationMethodsParams
{
    [OnSerializing]
    protected abstract void OnSerializingMethod(StreamingContext context, object otherPar); // Compliant, despite wrong params, it is in interface

    [OnSerialized]
    protected abstract void OnSerializedMethod(object otherPar);

    [OnDeserializing]
    protected void OnDeserializingMethod(object otherPar, StreamingContext context) { }

    [OnDeserialized]
    protected virtual void OnDeserializedMethod() { }
}


[Serializable]
public record Foo
{
    [OnSerializing]
    public void OnSerializing(StreamingContext context) { } // Noncompliant {{Make this method non-public.}}
//              ^^^^^^^^^^^^^

    [OnSerialized]
    int OnSerialized(StreamingContext context) { return 1; } // Noncompliant {{Make this method return 'void'.}}

    [OnDeserializing]
    void OnDeserializing() { } // Noncompliant  {{Make this method have a single parameter of type 'StreamingContext'.}}

    [OnDeserialized]
    void OnDeserialized(StreamingContext context, string str) { } // Noncompliant {{Make this method have a single parameter of type 'StreamingContext'.}}

    void OnDeserialized2(StreamingContext context, string str) { } // Compliant

    [OnDeserialized]
    void OnDeserialized<T>(StreamingContext context) { } // Noncompliant {{Make this method have no type parameters.}}

    [OnDeserializing]
    public int OnDeserializing2(StreamingContext context) { throw new NotImplementedException(); } // Noncompliant {{Make this method non-public and return 'void'.}}

    [OnDeserializing]
    public void OnDeserializing3() { throw new NotImplementedException(); } // Noncompliant {{Make this method non-public and have a single parameter of type 'StreamingContext'.}}

    [OnDeserializing]
    int OnDeserializing4() { throw new NotImplementedException(); } // Noncompliant {{Make this method return 'void' and have a single parameter of type 'StreamingContext'.}}

    [OnDeserializing]
    public int OnDeserializing5<T>() { throw new NotImplementedException(); } // Noncompliant {{Make this method non-public, return 'void', have no type parameters and have a single parameter of type 'StreamingContext'.}}

    [OnSerializing]
    private static void OnSerializingStatic(StreamingContext context) { } // Noncompliant {{Make this method non-static.}}

    [OnSerializing]
    public static void OnSerializingPublicStatic(StreamingContext context) { } // Noncompliant {{Make this method non-public and non-static.}}

    [OnSerializing()]
    internal void OnSerializingMethod(StreamingContext context) { }     // Compliant, method is not public and gets invoked

    [OnSerialized()]
    protected void OnSerializedMethod(StreamingContext context) { }      // Compliant, method is not public and gets invoked

    [OnDeserializing()]
    private void OnDeserializingMethod(StreamingContext context) { }    // Compliant

    [OnDeserialized()]
    private void OnDeserializedMethod(StreamingContext context) { }     // Compliant

    public void LocalFunctions()
    {
        [OnSerializing()] // Noncompliant {{Serialization attributes on local functions are not considered.}}
        void OnSerializing(StreamingContext context)
        { }

        [OnSerialized()] // Noncompliant {{Serialization attributes on local functions are not considered.}}
        void OnSerialized(StreamingContext context)
        { }

        [OnDeserializing()] // Noncompliant {{Serialization attributes on local functions are not considered.}}
        void OnDeserializing(StreamingContext context)
        { }

        [OnDeserialized()] // Noncompliant {{Serialization attributes on local functions are not considered.}}
        void OnDeserialized(StreamingContext context)
        { }

        [OnDeserialized()] // Noncompliant {{Serialization attributes on local functions are not considered.}}
        static void OnDeserializedStatic(StreamingContext context)
        { }
    }
}

[Serializable]
public partial class Partial
{
    [OnSerialized]
    private partial int OnSerialized(StreamingContext context);             // Noncompliant

    [OnDeserialized()]
    private partial void OnDeserializedMethod(StreamingContext context);    // Compliant
}

public partial class Partial
{
    private partial int OnSerialized(StreamingContext context) => 42;           // Noncompliant

    private partial void OnDeserializedMethod(StreamingContext context) { }     // Compliant
}

internal class TestCases
{
    public void Method(IEnumerable<int> collection)
    {
        [OnSerializing] int Get() => 1; // Noncompliant {{Serialization attributes on local functions are not considered.}}

        _ = collection.Select([OnSerialized] (x) => x + 1);  // Noncompliant {{Serialization attributes on lambdas are not considered.}}

        Action a = [OnDeserializing] () => { }; // Noncompliant

        Action x = true
                       ? ([OnDeserialized] () => { }) // Noncompliant
                       : [OnDeserialized] () => { };  // Noncompliant

        Call([OnDeserialized] (x) => { }); // Noncompliant
    }

    private void Call(Action<int> action) => action(1);
}

[Serializable]
internal class SerializableWithGenericAttribute
{
    [OnSerializing, Generic<int>]
    public void OnSerializing(StreamingContext context) { } // Noncompliant
}

public class GenericAttribute<T> : Attribute { }

interface IMyInterface
{
    [OnSerializing]
    static virtual void OnSerializingStaticVirtual(StreamingContext context) { } // Compliant, because in an interface

    [OnSerializing]
    static abstract void OnSerializingStaticAbstract(StreamingContext context); // Compliant, because in an interface
}
