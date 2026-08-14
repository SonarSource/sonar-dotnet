using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using KeyedServices = Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute;

// https://sonarsource.atlassian.net/browse/NET-4245

public class PrimaryBase(object service);
public class PrimaryMultipleBase(object first, object second);

public class PrimaryDerived([FromKeyedServices("key")] object service, int p1, int p2, int p3, int p4) : PrimaryBase(service);   // Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}

public class PrimaryDerivedCompliant([FromKeyedServices("key")] object service, int p1, int p2, int p3) : PrimaryBase(service);  // Compliant

public class PrimaryDerivedRegular(object service, int p1, int p2, int p3, int p4) : PrimaryBase(service);                       // Noncompliant {{Constructor has 4 new parameters, which is greater than the 3 authorized.}}

public class PrimaryStandalone([FromKeyedServices("key")] object service, int p1, int p2, int p3);                               // Compliant

public struct PrimaryStruct([FromKeyedServices("key")] object service, int p1, int p2, int p3);                                  // Compliant

public class PrimaryDerivedMultiple([FromServices] object first, [FromKeyedServices("key")] object second, int p1, int p2, int p3, int p4) : PrimaryMultipleBase(first, second); // Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}

public class FullyQualifiedAndAliased([Microsoft.AspNetCore.Mvc.FromServices] object first, [KeyedServices("key")] object second, int p1, int p2, int p3); // Compliant

public class DerivedFromServicesAttribute : FromServicesAttribute { }
public class DerivedFromKeyedServicesAttribute(object key) : FromKeyedServicesAttribute(key) { }

public class InheritedAttributes([DerivedFromServices] object first, [DerivedFromKeyedServices("key")] object second, int p1, int p2, int p3); // Compliant

public class ThisChaining
{
    public ThisChaining([FromServices] object service, int p1, int p2, int p3, int p4) : this(service) { } // Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}

    private ThisChaining(object service) { }
}

public class LocalFunctions
{
    public void Method()
    {
        void Excludes([FromKeyedServices("key")] object service, int p1, int p2, int p3) { }                                     // Compliant
        void Counts([FromKeyedServices("key")] object service, int p1, int p2, int p3, int p4) { }                               // Noncompliant {{Local function has 4 parameters, which is greater than the 3 authorized.}}

        Excludes(null, 1, 2, 3);
        Counts(null, 1, 2, 3, 4);
    }
}
