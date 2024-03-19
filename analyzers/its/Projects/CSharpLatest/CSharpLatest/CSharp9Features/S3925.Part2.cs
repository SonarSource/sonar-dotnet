using System.Runtime.Serialization;

namespace CSharpLatest.CSharp9Features;

public partial class Partial_SerializableDerived_Not_CallingBase_GetObjectData_SeparateFiles_Class
{
    public override partial void GetObjectData(SerializationInfo info, StreamingContext context) { }
}
