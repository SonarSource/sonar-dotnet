using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics.PartialMethods
{
    public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class
    //                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant [4] {{Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) { }  // Secondary [4] {{Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.}}
    }
}
