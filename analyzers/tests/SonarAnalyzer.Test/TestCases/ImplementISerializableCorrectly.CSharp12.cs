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

    [Serializable]
    public class SomeClass(Serializable serializableField) : Serializable
//               ^^^^^^^^^ Noncompliant
//         ^^^^^ Secondary@-1 {{Override 'GetObjectData(SerializationInfo, StreamingContext)' and serialize '<serializableField>P'.}}
    {
        public SomeClass() : this(new Serializable()) { }

        protected SomeClass(SerializationInfo info, StreamingContext context) : this(new Serializable()) { }
        //        ^^^^^^^^^ Secondary {{Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.}}
        private void SomeMethod()
        {
            Console.WriteLine(serializableField.ToString());
        }
    }
}
