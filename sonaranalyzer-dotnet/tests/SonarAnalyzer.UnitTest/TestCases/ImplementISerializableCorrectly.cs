using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    [Serializable]
    public class Serializable : ISerializable
    {
        public Serializable()
        { /*do something*/ }
        protected Serializable(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    public abstract class SerializableAbstract : ISerializable
    {
        public SerializableAbstract()
        { /*do something*/ }
        protected SerializableAbstract(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public sealed class SerializableSealed : ISerializable
    {
        public SerializableSealed()
        { /*do something*/ }
        private SerializableSealed(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    internal class SerializableInternal : ISerializable // Nonpublic classes are ignored
    {
        public SerializableInternal()
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public class SerializableDerived : Serializable
    {
        public SerializableDerived()
        { /*do something*/ }
        protected SerializableDerived(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { /*do something*/ }
    }

    [Serializable]
    public class SerializableDerived_1 : Serializable
    {
        private Serializable serializableField;
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

    public class Serializable_NoAttribute : ISerializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^ Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'Serializable_NoAttribute' because it implements 'ISerializable'.}}
    {
        public Serializable_NoAttribute()
        { /*do something*/ }
        protected Serializable_NoAttribute(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
//                  ^^^^^^^^^^^^^ Secondary {{Make 'GetObjectData' 'public' and 'virtual', or seal 'Serializable_NoAttribute'.}}
        { /*do something*/ }
    }

    public class Serializable_NoAttribute_1 : Serializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^ Secondary@-1 {{Add 'System.SerializableAttribute' attribute on 'Serializable_NoAttribute_1' because it implements 'ISerializable'.}}
    {
        public Serializable_NoAttribute_1()
        { /*do something*/ }
        protected Serializable_NoAttribute_1(SerializationInfo info, StreamingContext context)
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{Call constructor 'base(SerializationInfo, StreamingContext)'.}}
        { /*do something*/ }
    }

    [Serializable]
    public class Serializable_ExplicitImplementation : ISerializable // Compliant, False Negative - rule should be extended to ensure there is a virtual GetObjectData method that is called
    {
        public Serializable_ExplicitImplementation()
        { /*do something*/ }
        protected Serializable_ExplicitImplementation(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public class Serializable_ExplicitImplementation2 : ISerializable
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
    public sealed class Serializable_Sealed : ISerializable
//                      ^^^^^^^^^^^^^^^^^^^ Noncompliant
    {
        public Serializable_Sealed()
        { /*do something*/ }
        protected Serializable_Sealed(SerializationInfo info, StreamingContext context)
//                ^^^^^^^^^^^^^^^^^^^ Secondary {{Make this constructor 'private'.}}
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public sealed class Serializable_Sealed_NoConstructor : ISerializable
//                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//                ^^^^^ Secondary@-1 {{Add a 'private' constructor 'Serializable_Sealed_NoConstructor(SerializationInfo, StreamingContext)'.}}
    {
        public Serializable_Sealed_NoConstructor()
        { /*do something*/ }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public class Serializable_NoConstructor : ISerializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//         ^^^^^ Secondary@-1 {{Add a 'protected' constructor 'Serializable_NoConstructor(SerializationInfo, StreamingContext)'.}}
    {
        public Serializable_NoConstructor()
        { /*do something*/ }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        { /*do something*/ }
    }

    [Serializable]
    public class SerializableDerived_Not_CallingBase : Serializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
    {
        public SerializableDerived_Not_CallingBase()
        { /*do something*/ }
        protected SerializableDerived_Not_CallingBase(SerializationInfo info, StreamingContext context)
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary {{Call constructor 'base(SerializationInfo, StreamingContext)'.}}
        { /*do something*/ }
    }

    [Serializable]
    public class SerializableDerived_Not_CallingBase_GetObjectData : Serializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
    {
        private Serializable serializableField;
        public SerializableDerived_Not_CallingBase_GetObjectData()
        { /*do something*/ }
        protected SerializableDerived_Not_CallingBase_GetObjectData(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { /*do something*/ }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
//                           ^^^^^^^^^^^^^ Secondary {{Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in this method.}}
        { /*do something*/ }
    }

    [Serializable]
    public class SerializableDerived_New_GetObjectData : Serializable
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
    {
        public SerializableDerived_New_GetObjectData()
        { /*do something*/ }
        protected SerializableDerived_New_GetObjectData(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { /*do something*/ }

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
        public SerializableDerived_Not_Overriding_GetObjectData()
        { /*do something*/ }
        protected SerializableDerived_Not_Overriding_GetObjectData(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { /*do something*/ }
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
}
