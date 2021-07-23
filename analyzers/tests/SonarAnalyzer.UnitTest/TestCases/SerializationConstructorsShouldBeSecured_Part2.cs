using System.Runtime.Serialization;
using System.Security.Permissions;

namespace MyLibrary
{
    public partial class PartialFoo
    {
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        protected PartialFoo(SerializationInfo info, StreamingContext context) { }  // Noncompliant
    }

    public partial class PartialFoo_ok : ISerializable
    {
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected PartialFoo_ok(SerializationInfo info, StreamingContext context) { }
    }


    public partial class PartialFoo2 : ISerializable
    {
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public PartialFoo2() { }
    }

    public partial class PartialFoo2_ok : ISerializable
    {
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public PartialFoo2_ok() { }
    }
}
