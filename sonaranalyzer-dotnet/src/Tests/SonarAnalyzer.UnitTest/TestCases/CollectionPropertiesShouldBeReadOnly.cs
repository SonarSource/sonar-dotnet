using System;
using System.Collections;
using System.Collections.Generic;

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
}
