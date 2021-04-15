using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;

[assembly: System.Security.AllowPartiallyTrustedCallers()]
namespace MyLibrary
{
    [Serializable]
    public class Foo : ISerializable
    {
        private int n;

        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public Foo()
        {
            n = -1;
        }

        protected Foo(SerializationInfo info, StreamingContext context) // Noncompliant {{Secure this serialization constructor.}}
//                ^^^
        {
            n = (int)info.GetValue("n", typeof(int));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("n", n);
        }
    }

    [Serializable]
    public class Foo_ok : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public Foo_ok() { }

        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        protected Foo_ok(SerializationInfo info, StreamingContext context) { } // Compliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class Foo2 : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public Foo2() { }

        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        protected Foo2(SerializationInfo info, StreamingContext context) { } // Noncompliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }


    [Serializable]
    public class Foo2_ok : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public Foo2_ok() { }

        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected Foo2_ok(SerializationInfo info, StreamingContext context) { } // Compliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class Foo3 : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public Foo3() { }

        [GacIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public Foo3(int i) { }

        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected Foo3(SerializationInfo info, StreamingContext context) { } // Noncompliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class Foo3_ok : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public Foo3_ok() { }

        [GacIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        public Foo3_ok(int i) { }

        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        [GacIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        [ZoneIdentityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected Foo3_ok(SerializationInfo info, StreamingContext context) { } // Compliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }


    public class Foo4 : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.InheritanceDemand)]
        public Foo4() { }

        [FileIOPermissionAttribute(SecurityAction.Demand)]
        protected Foo4(SerializationInfo info, StreamingContext context) { } // Noncompliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public class Foo4_ok : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.InheritanceDemand)]
        public Foo4_ok() { }

        [FileIOPermissionAttribute(SecurityAction.InheritanceDemand)]
        protected Foo4_ok(SerializationInfo info, StreamingContext context) { } // Compliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }


    public class Foo5 : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = false)]
        public Foo5() { }

        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        protected Foo5(SerializationInfo info, StreamingContext context) { } // Noncompliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public class Foo5_ok : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand)]
        public Foo5_ok() { }

        [FileIOPermissionAttribute(SecurityAction.Demand)]
        protected Foo5_ok(SerializationInfo info, StreamingContext context) { } // Compliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }


    public class Foo6 : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public Foo6() { }

        [FileIOPermissionAttribute(SecurityAction.Demand)]
        protected Foo6(SerializationInfo info, StreamingContext context) { } // Noncompliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public class Foo6_ok : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public Foo6_ok() { }

        [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        protected Foo6_ok(SerializationInfo info, StreamingContext context) { } // Compliant

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public partial class MalformedAttributes : ISerializable
    {
        [FileIOPermissionAttribute(SecurityAction.Whatever_Mate)] // Error [CS0117] - invalid value
        public MalformedAttributes() { }

        protected MalformedAttributes(SerializationInfo info, StreamingContext context) { } // Noncompliant

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}
