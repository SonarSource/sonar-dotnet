using System;
using System.Runtime.Serialization;

namespace Net5
{
    [Serializable]
    public record SerializableRecord : ISerializable
    {
        public SerializableRecord() { }
        protected SerializableRecord(SerializationInfo info, StreamingContext context) { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public partial record Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record : SerializableRecord
    {
        public Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record() { }
        protected Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    public partial record Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class SerializableClass : ISerializable
    {
        public SerializableClass() { }
        protected SerializableClass(SerializationInfo info, StreamingContext context) { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class : SerializableClass
    {
        public Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class() { }
        protected Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}
