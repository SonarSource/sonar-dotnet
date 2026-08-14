using System;
using HotChocolate;

// https://sonarsource.atlassian.net/browse/NET-4245

namespace Other
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ServiceAttribute : Attribute { }
}

namespace Tests.Diagnostics
{
    public class Resolvers
    {
        public void ExcludesEveryInjectedParameter(
            [Service] object service,
            [GlobalState] object globalState,
            [ScopedState] object scopedState,
            [LocalState] object localState,
            [Parent] object parent,
            [EventMessage] object eventMessage,
            [SchemaService] object schemaService,
            [ScopedService] object scopedService,   // Obsolete since Hot Chocolate 13 and removed in 14
            int p1,
            int p2,
            int p3) { }                                                                                                 // Compliant, only 3 parameters are supplied by the caller

        public void CountsTheRemainingParameters([Service] object service, int p1, int p2, int p3, int p4) { }           // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}

        public void CountsSameNamedAttributeFromAnotherNamespace([Other.Service] object service, int p1, int p2, int p3) { }    // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}
    }
}
