using System;
using System.Runtime.Serialization;

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
