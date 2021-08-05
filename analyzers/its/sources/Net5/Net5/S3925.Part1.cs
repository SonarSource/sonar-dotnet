using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    [Serializable]
    public record SerializableRecord : ISerializable
    {
        public SerializableRecord() { }
        protected SerializableRecord(SerializationInfo info, StreamingContext context) { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public abstract record SerializableAbstract : ISerializable
    {
        public SerializableAbstract() { }
        protected SerializableAbstract(SerializationInfo info, StreamingContext context) { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public sealed record SerializableSealed : ISerializable
    {
        public SerializableSealed() { }
        private SerializableSealed(SerializationInfo info, StreamingContext context) { }
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    internal record SerializableInternal : ISerializable
    {
        public SerializableInternal() { }
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record SerializableDerived : SerializableRecord
    {
        public SerializableDerived() { }
        protected SerializableDerived(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public record SerializableDerived_1 : SerializableRecord
    {
        private SerializableRecord serializableField;

        public SerializableDerived_1() { }
        protected SerializableDerived_1(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    public record NonSerializedType
    {
    }

    public record Serializable_NoAttribute : ISerializable
    {
        private readonly NonSerializedType field;

        public Serializable_NoAttribute() { }
        protected Serializable_NoAttribute(SerializationInfo info, StreamingContext context) { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public record Serializable_NoAttribute_1 : SerializableRecord
    {
        public Serializable_NoAttribute_1() { }

        protected Serializable_NoAttribute_1(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record Serializable_ExplicitImplementation : ISerializable
    {
        public Serializable_ExplicitImplementation() { }
        protected Serializable_ExplicitImplementation(SerializationInfo info, StreamingContext context) { }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record Serializable_ExplicitImplementation2 : ISerializable
    {
        public Serializable_ExplicitImplementation2() { }
        protected Serializable_ExplicitImplementation2(SerializationInfo info, StreamingContext context) { }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        protected void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public sealed record Serializable_Sealed : ISerializable
    {
        public Serializable_Sealed() { }

        protected Serializable_Sealed(SerializationInfo info, StreamingContext context) { }
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public sealed record Serializable_Sealed_NoConstructor : ISerializable
    {
        public Serializable_Sealed_NoConstructor() { }
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record Serializable_NoConstructor : ISerializable
    {
        public Serializable_NoConstructor() { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record SerializableDerived_Not_CallingBase : SerializableRecord
    {
        public SerializableDerived_Not_CallingBase() { }

        protected SerializableDerived_Not_CallingBase(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record SerializableDerived_Not_CallingBase_GetObjectData : SerializableRecord
    {
        private SerializableRecord serializableField;
        public SerializableDerived_Not_CallingBase_GetObjectData() { }
        protected SerializableDerived_Not_CallingBase_GetObjectData(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record SerializableDerived_New_GetObjectData : SerializableRecord
    {
        public SerializableDerived_New_GetObjectData() { }
        protected SerializableDerived_New_GetObjectData(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public record SerializableDerived_Not_Overriding_GetObjectData : SerializableRecord
    {
        private SerializableRecord serializableField;

        public SerializableDerived_Not_Overriding_GetObjectData() { }
        protected SerializableDerived_Not_Overriding_GetObjectData(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public record Serializable_NoConstructor_Positional(string Value) : ISerializable
    {
        public Serializable_NoConstructor_Positional() : this("") { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}

namespace Tests.Diagnostics.PartialMethods
{
    [Serializable]
    public partial record Partial_SerializableDerived_CallingBase_GetObjectData_Record : SerializableRecord
    {
        public Partial_SerializableDerived_CallingBase_GetObjectData_Record() { }
        protected Partial_SerializableDerived_CallingBase_GetObjectData_Record(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    public partial record Partial_SerializableDerived_CallingBase_GetObjectData_Record
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) =>
            base.GetObjectData(info, context);
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
    public partial class Partial_SerializableDerived_CallingBase_GetObjectData_Class : SerializableClass
    {
        public Partial_SerializableDerived_CallingBase_GetObjectData_Class() { }
        protected Partial_SerializableDerived_CallingBase_GetObjectData_Class(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    public partial class Partial_SerializableDerived_CallingBase_GetObjectData_Class
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) =>
            base.GetObjectData(info, context);
    }

    [Serializable]
    public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class : SerializableClass
    {
        public Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class() { }
        protected Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public partial record Partial_SerializableDerived_Not_SerializableAttributeOnNonInheriting_Record : SerializableRecord
    {
        public Partial_SerializableDerived_Not_SerializableAttributeOnNonInheriting_Record() { }
        protected Partial_SerializableDerived_Not_SerializableAttributeOnNonInheriting_Record(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    [Serializable]
    public partial record Partial_SerializableDerived_Not_SerializableAttributeOnNonInheriting_Record
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) =>
            base.GetObjectData(info, context);
    }

    [Serializable]
    public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class : SerializableClass
    {
        public Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class() { }
        protected Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}
