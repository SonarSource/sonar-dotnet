using System;
using Orleans.Runtime;

// https://sonarsource.atlassian.net/browse/NET-4245

namespace Other
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class PersistentStateAttribute : Attribute { }
}

namespace Tests.Diagnostics
{
    public class Grains
    {
        // The grain state is injected by the Orleans runtime when the grain is activated, so it is not supplied by the caller
        public void ExcludesGrainState([PersistentState("state", "store")] object state, int p1, int p2, int p3) { }                     // Compliant, only 3 parameters are supplied by the caller

        public void CountsTheRemainingParameters([PersistentState("state", "store")] object state, int p1, int p2, int p3, int p4) { }   // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}

        public void CountsSameNamedAttributeFromAnotherNamespace([Other.PersistentState] object state, int p1, int p2, int p3) { }       // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}
    }
}
