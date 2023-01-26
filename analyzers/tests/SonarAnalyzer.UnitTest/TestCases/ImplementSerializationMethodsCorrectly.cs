using System;
using System.Runtime.Serialization;
using System.ComponentModel;

[Serializable]
public class Foo
{
    [OnSerializing]
    public void OnSerializing(StreamingContext context) { } // Noncompliant {{Make this method non-public.}}
    //          ^^^^^^^^^^^^^

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

    [OnDeserialized]
    [EditorBrowsable]
    public static void OnDeserialized(StreamingContext context) { }      // Noncompliant {{Make this method non-public and non-static.}}

    [OnDeserializing]
    [EditorBrowsable]
    public static void OnDeserializing(StreamingContext context) { }     // Noncompliant {{Make this method non-public and non-static.}}

    [OnDeserialized]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void OnDeserialized1(StreamingContext context) { }     // Noncompliant {{Make this method non-public and non-static.}}

    [OnDeserializing]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void OnDeserializing1(StreamingContext context) { }    // Noncompliant {{Make this method non-public and non-static.}}

    [OnSerializing()]
    internal void OnSerializingMethod(StreamingContext context) { }     // Compliant, method is not public and gets invoked

    [OnSerialized()]
    protected void OnSerializedMethod(StreamingContext context) { }      // Compliant, method is not public and gets invoked

    [OnSerializing]
    protected internal void OnProtectedInternal(StreamingContext context) { } // Compliant, method is not public and gets invoked

    [OnDeserializing()]
    private void OnDeserializingMethod(StreamingContext context) { }    // Compliant

    [OnDeserialized()]
    private void OnDeserializedMethod(StreamingContext context) { }     // Compliant

    [OnDeserialized]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void OnDeserialized3(StreamingContext context) { }           // Compliant

    [OnDeserializing]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void OnDeserializing3(StreamingContext context) { }          // Compliant

    [OnSerializing]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void OnSerializing3(StreamingContext context) { }            // Compliant

    [OnSerialized]
    [EditorBrowsable(EditorBrowsableState.Never)]
    int OnSerialized3(StreamingContext context) { return 1; }           // Compliant
}


