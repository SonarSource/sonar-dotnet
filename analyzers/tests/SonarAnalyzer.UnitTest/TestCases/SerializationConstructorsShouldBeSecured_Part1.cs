using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

[assembly: System.Security.AllowPartiallyTrustedCallers()]
namespace MyLibrary
{
    [Serializable]
    public partial class PartialFoo : ISerializable
    {
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public PartialFoo() { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public partial class PartialFoo_ok : ISerializable
    {
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public PartialFoo_ok() { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }


    [Serializable]
    public partial class PartialFoo2 : ISerializable
    {
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        protected PartialFoo2(SerializationInfo info, StreamingContext context) { } // Noncompliant

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public partial class PartialFoo2_ok : ISerializable
    {
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected PartialFoo2_ok(SerializationInfo info, StreamingContext context) { }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}
