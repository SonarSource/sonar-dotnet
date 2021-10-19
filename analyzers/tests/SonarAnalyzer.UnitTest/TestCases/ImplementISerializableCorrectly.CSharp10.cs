using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    public class NonSerializedType
    {
    }

    public struct SerializableStruct_NoAttribute : ISerializable // FN
    {
        private readonly NonSerializedType field = null; // FN, should be marked with [NonSerialized]

        public SerializableStruct_NoAttribute() { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public record struct SerializableRecordStruct_NoAttribute : ISerializable // FN
    {
        private readonly NonSerializedType field = null; // FN, should be marked with [NonSerialized]

        public SerializableRecordStruct_NoAttribute() { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public record struct SerializablePositionalRecordStruct_NoAttribute(string Property) : ISerializable // FN
    {
        private readonly NonSerializedType field = null; // FN, should be marked with [NonSerialized]

        public SerializablePositionalRecordStruct_NoAttribute() : this ("SomeString") { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}
