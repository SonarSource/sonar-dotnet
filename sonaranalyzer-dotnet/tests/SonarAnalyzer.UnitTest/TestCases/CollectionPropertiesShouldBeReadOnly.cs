using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    public class Program
    {
        private List<string> list;

        public ArrayList NongenericList { get; set; } // Noncompliant {{Make the 'NongenericList' property read-only by removing the property setter or making it private.}}
//                       ^^^^^^^^^^^^^^
        public ICollection NongenericCollection { get; set; } // Noncompliant {{Make the 'NongenericCollection' property read-only by removing the property setter or making it private.}}
        public IEnumerable NongenericEnumerable { get; set; }
        public List<string> GenericList { get; set; } // Noncompliant
        public ICollection<string> GenericCollection { get; set; } // Noncompliant
        public IEnumerable<string> GenericEnumerable { get; set; }

        protected ICollection<string> GenericCollectionProtected { get; set; } // Noncompliant

        internal ICollection<string> GenericCollectionInternal { get; set; }
        private ICollection<string> GenericCollectionPrivate { get; set; }

        public ICollection<string> GenericCollectionNoSetAuto { get; }
        public ICollection<string> GenericCollectionNoSet { get { return list; } }
        public ICollection<string> GenericCollectionArrow => list;
        public ICollection<string> GenericCollectionPrivateSet { get; private set; }
        public ICollection<string> GenericCollectionInternalSet { get; internal set; }
        public string StringProperty { get; set; }
        public string[] ArrayProperty { get; set; }
        public System.Security.PermissionSet PermissionSetProperty { get; set; }

        private class PrivateClass
        {
            public ArrayList NongenericList { get; set; }
        }

        internal class InternalClass
        {
            public ArrayList NongenericList { get; set; }
        }

        private interface PrivateInterface
        {
            ArrayList NongenericList { get; set; }
        }
    }

    public interface PublicInterface
    {
        ArrayList NongenericList { get; set; } // Noncompliant
        List<string> GenericList { get; set; } // Noncompliant
    }

    // Ignore collections marked with DataMember attribute: https://github.com/SonarSource/sonar-dotnet/issues/795
    [DataContract]
    public class Message
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string MessageBody { get; set; }

        [DataMember]
        public Dictionary<string, string> Properties { get; set; }

        [System.Runtime.Serialization.DataMember]
        public List<int> Values { get; set; }
    }

    // Ignore collections marked with Serializable attribute: https://github.com/SonarSource/sonar-dotnet/issues/2762
    [Serializable]
    public class SerializableMessage
    {
        public Dictionary<string, string> Properties { get; set; }

        public List<int> Values { get; set; }
    }

    public abstract class S4004Base
    {
        public abstract IDictionary<object, object> Items { get; set; } // Noncompliant
    }

    public class S4004Abstract : S4004Base
    {
        public override IDictionary<object, object> Items { get; set; } // Compliant enforced by base (https://github.com/SonarSource/sonar-dotnet/issues/2606)
    }

    public interface IS4004
    {
        IDictionary<object, object> Items { get; set; } // Noncompliant
    }

    public class S4004InterfaceImplicit : IS4004
    {
        public IDictionary<object, object> Items { get; set; }  // Compliant enforced by interface (https://github.com/SonarSource/sonar-dotnet/issues/2606)
    }

    public class S4004InterfaceExplicit : IS4004
    {
        IDictionary<object, object> IS4004.Items { get; set; }  // Compliant enforced by interface (https://github.com/SonarSource/sonar-dotnet/issues/2606)
    }
}
