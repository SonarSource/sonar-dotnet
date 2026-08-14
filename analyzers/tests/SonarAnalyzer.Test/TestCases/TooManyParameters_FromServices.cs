using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

// https://sonarsource.atlassian.net/browse/NET-4245

namespace Other
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FromServicesAttribute : Attribute { }
}

namespace Tests.Diagnostics
{
    public class Methods
    {
        public void ExcludesServices([FromServices] object service, int p1, int p2, int p3) { }                                 // Compliant, the injected parameter is not counted

        public void ExcludesKeyedServices([FromKeyedServices("key")] object service, int p1, int p2, int p3) { }                // Compliant

        public void ExcludesEveryInjectedParameter([FromServices] object first, [FromKeyedServices("key")] object second, int p1, int p2, int p3) { }   // Compliant

        public void CountsTheRemainingParameters([FromServices] object service, int p1, int p2, int p3, int p4) { }             // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}

        public void CountsSameNamedAttributeFromAnotherNamespace([Other.FromServices] object value, int p1, int p2, int p3) { } // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}

        public void CountsUnannotatedParameters(object service, int p1, int p2, int p3) { }                                     // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}
    }

    public class Base
    {
        public Base(object service) { }
    }

    public class Derived : Base
    {
        // The injected parameter is already excluded from the parameter count, so it must not be subtracted a second time as a base constructor argument
        public Derived([FromServices] object service, int p1, int p2, int p3, int p4) : base(service) { }                       // Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}

        public Derived([FromServices] object service, int p1, int p2, int p3) : base(service) { }                               // Compliant

        public Derived([FromServices] object service, object forwarded, int p1, int p2, int p3) : base(forwarded) { }           // Compliant, only 'forwarded' is subtracted as a base constructor argument
    }

    public class DerivedRegular : Base
    {
        public DerivedRegular(object service, int p1, int p2, int p3, int p4) : base(service) { }                               // Noncompliant {{Constructor has 4 new parameters, which is greater than the 3 authorized.}}
    }
}
