using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    [Serializable]
    public record SerializableRecord : ISerializable
    {
        public SerializableRecord()
        { /*do something*/ }
        protected SerializableRecord(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public class SerializableClass : ISerializable
    {
        public SerializableClass()
        { /*do something*/ }
        protected SerializableClass(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    public abstract record SerializableAbstract : ISerializable
    {
        public SerializableAbstract()
        { /*do something*/ }
        protected SerializableAbstract(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public sealed record SerializableSealed : ISerializable
    {
        public SerializableSealed()
        { /*do something*/ }
        private SerializableSealed(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    internal record SerializableInternal : ISerializable // Nonpublic types are ignored
    {
        public SerializableInternal()
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public record SerializableDerived : SerializableRecord
    {
        public SerializableDerived()
        { /*do something*/ }
        protected SerializableDerived(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { /*do something*/ }
    }

    [Serializable]
    public record SerializableDerived_1 : SerializableRecord
    {
        private SerializableRecord serializableField;
        public SerializableDerived_1()
        { /*do something*/ }
        protected SerializableDerived_1(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { /*do something*/ }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            /*do something*/
            base.GetObjectData(info, context);
        }
    }

    public record Serializable_NoAttribute : ISerializable
    // FN, Should be Non-compliant
    // FN, should be second location with @ -1 Add 'System.SerializableAttribute' attribute on 'Serializable_NoAttribute' because it implements 'ISerializable'.}}
    {
        public Serializable_NoAttribute()
        { /*do something*/ }
        protected Serializable_NoAttribute(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        //FN, should be second location with Make 'GetObjectData' 'public' and 'virtual', or seal 'Serializable_NoAttribute'.}}
        { /*do something*/ }
    }

    public record Serializable_NoAttribute_1 : SerializableRecord
    // FN, Should be Non-compliant
    // FN, should be second location with @ -1 Add 'System.SerializableAttribute' attribute on 'Serializable_NoAttribute_1' because it implements 'ISerializable'.}}
    {
        public Serializable_NoAttribute_1()
        { /*do something*/ }
        protected Serializable_NoAttribute_1(SerializationInfo info, StreamingContext context)
        //FN, should be second location with Call constructor 'base(SerializationInfo, StreamingContext)'.}}
        { /*do something*/ }
    }

    [Serializable]
    public record Serializable_ExplicitImplementation : ISerializable // Compliant, False Negative - rule should be extended to ensure there is a virtual GetObjectData method that is called
    {
        public Serializable_ExplicitImplementation()
        { /*do something*/ }
        protected Serializable_ExplicitImplementation(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public record Serializable_ExplicitImplementation2 : ISerializable
    {
        public Serializable_ExplicitImplementation2()
        { /*do something*/ }
        protected Serializable_ExplicitImplementation2(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        { GetObjectData(info, context); }
        protected void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public sealed record Serializable_Sealed : ISerializable
    // FN, Should be Non-compliant
    {
        public Serializable_Sealed()
        { /*do something*/ }
        protected Serializable_Sealed(SerializationInfo info, StreamingContext context)
        //FN, should be second location with Make this constructor 'private'.}}
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public sealed record Serializable_Sealed_NoConstructor : ISerializable
    // FN, Should be Non-compliant
    //        FN, should be second location with @ -1 Add a 'private' constructor 'Serializable_Sealed_NoConstructor(SerializationInfo, StreamingContext)'.}}
    {
        public Serializable_Sealed_NoConstructor()
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public record Serializable_NoConstructor : ISerializable
    // FN, Should be Non-compliant
    // FN, should be second location with @ -1 Add a 'protected' constructor 'Serializable_NoConstructor(SerializationInfo, StreamingContext)'.}}
    {
        public Serializable_NoConstructor()
        { /*do something*/ }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public record SerializableDerived_Not_CallingBase : SerializableRecord
    // FN, Should be Non-compliant
    {
        public SerializableDerived_Not_CallingBase()
        { /*do something*/ }
        protected SerializableDerived_Not_CallingBase(SerializationInfo info, StreamingContext context)
        //FN, should be second location with Call constructor 'base(SerializationInfo, StreamingContext)'.}}
        { /*do something*/ }
    }

    [Serializable]
    public record SerializableDerived_Not_CallingBase_GetObjectData : SerializableRecord
    // FN, Should be Non-compliant
    {
        private SerializableRecord serializableField;
        public SerializableDerived_Not_CallingBase_GetObjectData()
        { /*do something*/ }
        protected SerializableDerived_Not_CallingBase_GetObjectData(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { /*do something*/ }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //FN, should be second location with Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in this method.}}
        { /*do something*/ }
    }

    [Serializable]
    public record SerializableDerived_New_GetObjectData : SerializableRecord
    // FN, Should be Non-compliant
    {
        public SerializableDerived_New_GetObjectData()
        { /*do something*/ }
        protected SerializableDerived_New_GetObjectData(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { /*do something*/ }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        //FN, should be second location with Make 'GetObjectData' 'public' and 'virtual', or seal 'SerializableDerived_New_GetObjectData'.}}
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public record SerializableDerived_Not_Overriding_GetObjectData : SerializableRecord
    // FN, Should be Non-compliant
    // FN, should be second location with @ -1 Override 'GetObjectData(SerializationInfo, StreamingContext)' and serialize 'serializableField'.}}
    {
        private SerializableRecord serializableField;
        public SerializableDerived_Not_Overriding_GetObjectData()
        { /*do something*/ }
        protected SerializableDerived_Not_Overriding_GetObjectData(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { /*do something*/ }
    }

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
    // FN, Should be Non-compliant
    {
        public Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record() { }
        protected Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    public partial record Partial_SerializableDerived_Not_CallingBase_GetObjectData_Record
    // FN, Should be Non-compliant
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context)
        //FN, should be second location with Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in this method.}}
        { /*do something*/ }
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
    // FN, Should be Non-compliant
    {
        public Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class() { }
        protected Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_Class
    // FN, Should be Non-compliant
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context)
        //FN, should be second location with Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in this method.}}
        { /*do something*/ }
    }
}
