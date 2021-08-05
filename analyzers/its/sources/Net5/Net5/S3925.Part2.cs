using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics.PartialMethods
{
    public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}
