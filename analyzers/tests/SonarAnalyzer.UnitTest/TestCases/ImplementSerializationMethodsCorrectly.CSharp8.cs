using System;
using System.Runtime.Serialization;

public interface IWithValidSerializationMethods
{
    [OnSerializing]
    protected abstract void OnSerializingMethod(StreamingContext context); // Compliant

    [OnSerialized]
    protected abstract void OnSerializedMethod(StreamingContext context); // Compliant

    [OnDeserializing]
    protected void OnDeserializingMethod(StreamingContext context) { } // Compliant

    [OnDeserialized]
    protected virtual void OnDeserializedMethod(StreamingContext context) { } // Compliant
}

public interface IWithInvalidSerializationMethodsModifiers
{
    [OnSerializing]
    public abstract void OnSerializingMethod(StreamingContext context); // Compliant

    [OnSerialized]
    public abstract void OnSerializedMethod(StreamingContext context); // Compliant

    [OnDeserializing]
    public void OnDeserializingMethod(StreamingContext context) { } // Compliant

    [OnDeserialized]
    public virtual void OnDeserializedMethod(StreamingContext context) { } // Compliant
}

public interface IWithInvalidSerializationMethodsParams
{
    [OnSerializing]
    protected abstract void OnSerializingMethod(StreamingContext context, object otherPar); // Compliant

    [OnSerialized]
    protected abstract void OnSerializedMethod(object otherPar); // Compliant

    [OnDeserializing]
    protected void OnDeserializingMethod(object otherPar, StreamingContext context) { } // Compliant

    [OnDeserialized]
    protected virtual void OnDeserializedMethod() { } // Compliant
}
