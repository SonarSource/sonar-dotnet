using System.Runtime.Serialization;

namespace Net5
{
    public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class
    {
        public override partial void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}
