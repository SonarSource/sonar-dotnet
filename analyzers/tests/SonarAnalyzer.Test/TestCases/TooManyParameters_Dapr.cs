using System;
using Microsoft.AspNetCore.Mvc;

// https://sonarsource.atlassian.net/browse/NET-4245

namespace Other
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FromStateAttribute : Attribute { }
}

namespace Tests.Diagnostics
{
    public sealed class DerivedFromStateAttribute : FromStateAttribute
    {
        public DerivedFromStateAttribute(string storeName) : base(storeName) { }
    }

    public class Controllers
    {
        // The state entry is bound by Dapr from the state store, so it is not supplied by the caller
        public void ExcludesState([FromState("statestore")] object state, int p1, int p2, int p3) { }                        // Compliant, only 3 parameters are supplied by the caller

        public void CountsTheRemainingParameters([FromState("statestore")] object state, int p1, int p2, int p3, int p4) { } // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}

        public void ExcludesDerivedState([DerivedFromState("statestore")] object state, int p1, int p2, int p3) { }          // Compliant, derived binding attributes are also supplied by Dapr

        public void CountsSameNamedAttributeFromAnotherNamespace([Other.FromState] object state, int p1, int p2, int p3) { } // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}
    }
}
