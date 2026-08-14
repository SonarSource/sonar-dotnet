using System;
using Microsoft.Azure.WebJobs;

// https://sonarsource.atlassian.net/browse/NET-4245

namespace Other
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class BlobAttribute : Attribute { }
}

namespace Tests.Diagnostics
{
    public class Functions
    {
        // Input bindings are resolved by the Functions runtime, so they are not supplied by the caller
        public void ExcludesEveryInputBinding(
            [Blob("container/blob")] object blob,
            [CosmosDB("database", "container")] object document,
            int p1,
            int p2,
            int p3) { }                                                                                                     // Compliant, only 3 parameters are supplied by the caller

        public void CountsTheRemainingParameters([Blob("container/blob")] object blob, int p1, int p2, int p3, int p4) { }   // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}

        public void CountsSameNamedAttributeFromAnotherNamespace([Other.Blob] object blob, int p1, int p2, int p3) { }       // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}

        // Trigger bindings are also supplied by the runtime, but they are out of scope here: only the input bindings listed in NET-4245 are excluded
        public void CountsTriggerBindings([BlobTrigger("container/{name}")] object blob, int p1, int p2, int p3) { }         // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}
    }
}
