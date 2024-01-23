using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    public class NonSerializedType
    {
    }

    public struct SerializableStruct_NoAttribute : ISerializable    // Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Add 'System.SerializableAttribute' attribute on 'SerializableStruct_NoAttribute' because it implements 'ISerializable'. Add a 'private' constructor 'SerializableStruct_NoAttribute(SerializationInfo, StreamingContext)'.}}
                                                                    // Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'SerializableStruct_NoAttribute' because it implements 'ISerializable'.}}
                                                                    // Secondary@-2 {{Add a 'private' constructor 'SerializableStruct_NoAttribute(SerializationInfo, StreamingContext)'.}}
    {
        private readonly NonSerializedType field = null; // Should be marked with [NonSerialized]

        public SerializableStruct_NoAttribute() { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public record struct SerializableRecordStruct_NoAttribute : ISerializable // Noncompliant
                                                                              // Secondary@-1
                                                                              // Secondary@-2
    {
        private readonly NonSerializedType field = null; // Should be marked with [NonSerialized]

        public SerializableRecordStruct_NoAttribute() { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public record struct SerializablePositionalRecordStruct_NoAttribute(string Property) : ISerializable // Noncompliant
                                                                                                         // Secondary@-1
                                                                                                         // Secondary@-2
    {
        private readonly NonSerializedType field = null; // Should be marked with [NonSerialized]

        public SerializablePositionalRecordStruct_NoAttribute() : this ("SomeString") { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}
