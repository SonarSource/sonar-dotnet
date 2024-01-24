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

    [Serializable]
    public record SerializableRecord_Positional(SerializableRecord PositionalSerializableField) : ISerializable
    {
        public SerializableRecord_Positional() : this((SerializableRecord)null) { }
        protected SerializableRecord_Positional(SerializationInfo info, StreamingContext context) : this((SerializableRecord)null) { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record SerializableRecord_PositionalNonserializable(NonSerializedType PositionalNonserializableField) : ISerializable    // FN
    {
        public SerializableRecord_PositionalNonserializable() : this((NonSerializedType)null) { }
        protected SerializableRecord_PositionalNonserializable(SerializationInfo info, StreamingContext context) : this((NonSerializedType)null) { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record SerializableRecord_PositionalNonserializable_AttributeOnField([field:NonSerialized]NonSerializedType PositionalNonserializableField) : ISerializable  // Compliant
    {
        public SerializableRecord_PositionalNonserializable_AttributeOnField() : this((NonSerializedType)null) { }
        protected SerializableRecord_PositionalNonserializable_AttributeOnField(SerializationInfo info, StreamingContext context) : this((NonSerializedType)null) { }
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

    internal record SerializableInternal : ISerializable // Nonpublic types are ignored
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
//                ^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^^ Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'Serializable_NoAttribute' because it implements 'ISerializable'.}}
    {
        private readonly NonSerializedType field; // FN, should be marked with [NonSerialized]

        public Serializable_NoAttribute() { }
        protected Serializable_NoAttribute(SerializationInfo info, StreamingContext context) { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
//                  ^^^^^^^^^^^^^ Secondary {{Make 'GetObjectData' 'public' and 'virtual', or seal 'Serializable_NoAttribute'.}}
    }

    public record Serializable_NoAttribute_1 : SerializableRecord, ISerializable
    //            ^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Add 'System.SerializableAttribute' attribute on 'Serializable_NoAttribute_1' because it implements 'ISerializable'. Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
    //     ^^^^^^ Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'Serializable_NoAttribute_1' because it implements 'ISerializable'.}}
    {
        public Serializable_NoAttribute_1() { }

        protected Serializable_NoAttribute_1(SerializationInfo info, StreamingContext context) { }
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
    }

    [Serializable]
    public record Serializable_ExplicitImplementation : ISerializable // Compliant, False Negative - rule should be extended to ensure there is a virtual GetObjectData method that is called
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
    //                   ^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Make the serialization constructor 'private'.}}
    {
        public Serializable_Sealed() { }

        protected Serializable_Sealed(SerializationInfo info, StreamingContext context) { }
        //        ^^^^^^^^^^^^^^^^^^^ Secondary {{Make the serialization constructor 'private'.}}
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public sealed record Serializable_Sealed_NoConstructor : ISerializable
//                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//                ^^^^^^ Secondary@-1 {{Add a 'private' constructor 'Serializable_Sealed_NoConstructor(SerializationInfo, StreamingContext)'.}}
    {
        public Serializable_Sealed_NoConstructor() { }
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record Serializable_NoConstructor : ISerializable
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^^ Secondary@-1 {{Add a 'protected' constructor 'Serializable_NoConstructor(SerializationInfo, StreamingContext)'.}}
    {
        public Serializable_NoConstructor() { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public record SerializableDerived_Not_CallingBase : SerializableRecord
    //            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
    {
        public SerializableDerived_Not_CallingBase() { }

        protected SerializableDerived_Not_CallingBase(SerializationInfo info, StreamingContext context) { }
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
    }

    [Serializable]
    public record SerializableDerived_Not_CallingBase_GetObjectData : SerializableRecord
    //            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    {
        private SerializableRecord serializableField;
        public SerializableDerived_Not_CallingBase_GetObjectData() { }
        protected SerializableDerived_Not_CallingBase_GetObjectData(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) { }
        //                   ^^^^^^^^^^^^^ Secondary {{Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    }

    [Serializable]
    public record SerializableDerived_New_GetObjectData : SerializableRecord
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
    {
        public SerializableDerived_New_GetObjectData() { }
        protected SerializableDerived_New_GetObjectData(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
//                      ^^^^^^^^^^^^^ Secondary {{Make 'GetObjectData' 'public' and 'virtual', or seal 'SerializableDerived_New_GetObjectData'.}}
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public record SerializableDerived_Not_Overriding_GetObjectData : SerializableRecord
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^^ Secondary@-1 {{Override 'GetObjectData(SerializationInfo, StreamingContext)' and serialize 'serializableField'.}}
    {
        private SerializableRecord serializableField;

        public SerializableDerived_Not_Overriding_GetObjectData() { }
        protected SerializableDerived_Not_Overriding_GetObjectData(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public record Serializable_NoConstructor_Positional(string Value) : ISerializable
    //            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Add a 'protected' constructor 'Serializable_NoConstructor_Positional(SerializationInfo, StreamingContext)'.}}
    //     ^^^^^^ Secondary@-1 {{Add a 'protected' constructor 'Serializable_NoConstructor_Positional(SerializationInfo, StreamingContext)'.}}
    {
        public Serializable_NoConstructor_Positional() : this("") { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}

namespace Tests.Diagnostics.PartialMethods
{
// See https://github.com/SonarSource/sonar-dotnet/issues/4411 Applicable to all raised issues below
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
    //                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    {
        public Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record() { }
        protected Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);     // Secondary
    }

    public partial record Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record
    //                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) { }      // Secondary
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
    //                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant [1] {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    {
        public Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class() { }
        protected Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context); // Secondary [1] {{Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    }

    public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class
    //                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant [2] {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) { }  // Secondary [2] {{Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
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
    //                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant [3] {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    {
        public Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class() { }
        protected Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context); // Secondary [3] {{Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    }
}
