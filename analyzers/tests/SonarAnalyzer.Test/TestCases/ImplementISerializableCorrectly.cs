using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tests.Diagnostics;

namespace Tests.Diagnostics
{
    [Serializable]
    public class Serializable : ISerializable
    {
        public Serializable() { }
        protected Serializable(SerializationInfo info, StreamingContext context) { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public abstract class SerializableAbstract : ISerializable
    {
        public SerializableAbstract() { }
        protected SerializableAbstract(SerializationInfo info, StreamingContext context) { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public sealed class SerializableSealed : ISerializable
    {
        public SerializableSealed() { }
        private SerializableSealed(SerializationInfo info, StreamingContext context) { }
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    internal class SerializableInternal : ISerializable // Nonpublic classes are ignored
    {
        public SerializableInternal() { }
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class SerializableDerived : Serializable
    {
        public SerializableDerived() { }
        protected SerializableDerived(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class SerializableDerived_1 : Serializable
    {
        private Serializable serializableField;
        public SerializableDerived_1() { }
        protected SerializableDerived_1(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    public class NonSerializedType
    {
    }

    public class Serializable_NoAttribute : ISerializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^ Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'Serializable_NoAttribute' because it implements 'ISerializable'.}}
    {
        private readonly NonSerializedType field; // FN, should be marked with [NonSerialized]

        public Serializable_NoAttribute() { }
        protected Serializable_NoAttribute(SerializationInfo info, StreamingContext context) { }
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
//                  ^^^^^^^^^^^^^ Secondary {{Make 'GetObjectData' 'public' and 'virtual', or seal 'Serializable_NoAttribute'.}}
    }

    public class Serializable_NoAttribute_1 : Serializable, ISerializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^ Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'Serializable_NoAttribute_1' because it implements 'ISerializable'.}}
    {
        public Serializable_NoAttribute_1() { }
        protected Serializable_NoAttribute_1(SerializationInfo info, StreamingContext context) { }
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
    }

    [Serializable]
    public class Serializable_ExplicitImplementation : ISerializable // Compliant, False Negative - rule should be extended to ensure there is a virtual GetObjectData method that is called
    {
        public Serializable_ExplicitImplementation() { }
        protected Serializable_ExplicitImplementation(SerializationInfo info, StreamingContext context) { }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class Serializable_ExplicitImplementation2 : ISerializable
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
    public sealed class Serializable_Sealed : ISerializable
    //                  ^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Make the serialization constructor 'private'.}}
    {
        public Serializable_Sealed() { }
        protected Serializable_Sealed(SerializationInfo info, StreamingContext context) { }
        //        ^^^^^^^^^^^^^^^^^^^ Secondary {{Make the serialization constructor 'private'.}}
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public sealed class Serializable_Sealed_NoConstructor : ISerializable
//                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//                ^^^^^ Secondary@-1 {{Add a 'private' constructor 'Serializable_Sealed_NoConstructor(SerializationInfo, StreamingContext)'.}}
    {
        public Serializable_Sealed_NoConstructor() { }
        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class Serializable_NoConstructor : ISerializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^ Secondary@-1 {{Add a 'protected' constructor 'Serializable_NoConstructor(SerializationInfo, StreamingContext)'.}}
    {
        public Serializable_NoConstructor() { }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class SerializableDerived_Not_CallingBase : Serializable
    //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
    {
        public SerializableDerived_Not_CallingBase() { }
        protected SerializableDerived_Not_CallingBase(SerializationInfo info, StreamingContext context) { }
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
    }

    [Serializable]
    public class SerializableDerived_Not_CallingBase_GetObjectData : Serializable
    //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    {
        private Serializable serializableField;

        public SerializableDerived_Not_CallingBase_GetObjectData() { }
        protected SerializableDerived_Not_CallingBase_GetObjectData(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { }
        //                   ^^^^^^^^^^^^^ Secondary {{Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    }

    [Serializable]
    public class SerializableDerived_Not_CallingBase_GetObjectData_Coverage : Serializable   // Noncompliant
    {
        private Serializable serializableField;

        public SerializableDerived_Not_CallingBase_GetObjectData_Coverage() { }
        protected SerializableDerived_Not_CallingBase_GetObjectData_Coverage(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) // Secondary {{Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
        {
            var somethingElse = new SerializableDerived_Not_CallingBase_GetObjectData_Coverage();
            somethingElse.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class SerializableDerived_New_GetObjectData : Serializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
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
    public class SerializableDerived_Not_Overriding_GetObjectData : Serializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^ Secondary@-1 {{Override 'GetObjectData(SerializationInfo, StreamingContext)' and serialize 'serializableField'.}}
    {
        private Serializable serializableField;

        public SerializableDerived_Not_Overriding_GetObjectData() { }
        protected SerializableDerived_Not_Overriding_GetObjectData(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class SerializableDerived_Not_Overriding_GetObjectData_NonserializableFields : Serializable
    {
        private NonSerializedAttribute nonSerializableField;

        public SerializableDerived_Not_Overriding_GetObjectData_NonserializableFields() { }
        protected SerializableDerived_Not_Overriding_GetObjectData_NonserializableFields(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class SerializableDerived_Not_Overriding_GetObjectData_StaticFields : Serializable
    {
        private static Serializable staticSerializable;

        public SerializableDerived_Not_Overriding_GetObjectData_StaticFields() { }
        protected SerializableDerived_Not_Overriding_GetObjectData_StaticFields(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class OptionException : Exception
    {
        public OptionException() { }
        public OptionException(string message) : base(message) { }
        public OptionException(string message, Exception innerException) : base(message, innerException) { }
        protected OptionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class OptionUnknownException : OptionException
    {
        private string option;

        public OptionUnknownException() { }

        public OptionUnknownException(string option)
            : base("Unknown option '" + option + "'")
        {
            this.option = option;
        }
        public OptionUnknownException(string option, Exception innerException)
            : base("Unknown option '" + option + "'", innerException)
        {
            this.option = option;
        }

        public OptionUnknownException(string option, string message) : base(message)
        {
            this.option = option;
        }

        protected OptionUnknownException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.option = info.GetString("option");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("option", this.option);
            base.GetObjectData(info, context);
        }

        public virtual String Option
        {
            get { return this.option; }
        }
    }

    public class MyException : Exception // Compliant: no opt-in for custom serialization
    { }

    [Serializable]
    public class SerializableDerived_NoExtraFields : Serializable
    {
        public SerializableDerived_NoExtraFields() { }
        protected SerializableDerived_NoExtraFields(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class CustomLookup : Dictionary<string, object> // Compliant: no opt-in for custom serialization
    {
    }

    public class CustomLookup_OptInPerInterface : Dictionary<string, object>, ISerializable
    //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
    //     ^^^^^                                Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'CustomLookup_OptInPerInterface' because it implements 'ISerializable'.}}
    //     ^^^^^                                Secondary@-2 {{Add a 'protected' constructor 'CustomLookup_OptInPerInterface(SerializationInfo, StreamingContext)'.}}
    {
    }

    [Serializable]
    public class CustomLookup_OptInPerAttribute : Dictionary<string, object>
    //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
    //     ^^^^^                                Secondary@-1 {{Add a 'protected' constructor 'CustomLookup_OptInPerAttribute(SerializationInfo, StreamingContext)'.}}
    {
    }

    public class CustomLookup_OptInPerConstructor1 : Dictionary<string, object>
    //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
    //     ^^^^^                                   Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'CustomLookup_OptInPerConstructor1' because it implements 'ISerializable'.}}
    {
        protected CustomLookup_OptInPerConstructor1(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }

    public class CustomLookup_OptInPerConstructor2 : Dictionary<string, object>
    //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Add 'System.SerializableAttribute' attribute on 'CustomLookup_OptInPerConstructor2' because it implements 'ISerializable'. Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
    //     ^^^^^                                    Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'CustomLookup_OptInPerConstructor2' because it implements 'ISerializable'.}}
    {
        protected CustomLookup_OptInPerConstructor2(SerializationInfo info, StreamingContext context)
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
        { }
    }
}
