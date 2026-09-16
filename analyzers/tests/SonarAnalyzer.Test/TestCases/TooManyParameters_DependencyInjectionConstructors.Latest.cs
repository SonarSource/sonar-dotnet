using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

[method: ActivatorUtilitiesConstructor]
public class ActivationConstructor(int p1, int p2, int p3, int p4) // Compliant
{
    public void Method(int p1, int p2, int p3, int p4) { } // Noncompliant

    public ActivationConstructor(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8) : this(0, 0, 0, 0) { } // Noncompliant
}

[method: ActivatorUtilitiesConstructor]
public struct ActivationStruct(int p1, int p2, int p3, int p4); // Compliant

[ApiController]
public class AnnotatedApiController(int p1, int p2, int p3, int p4); // Compliant

[Controller]
public class AnnotatedController(int p1, int p2, int p3, int p4); // Compliant

[Controller]
public class AnnotatedBase { }

public class InheritedController(int p1, int p2, int p3, int p4) : AnnotatedBase; // Compliant

[NonController]
public class OptedOutController(int p1, int p2, int p3, int p4) : Microsoft.AspNetCore.Mvc.ControllerBase; // Noncompliant

public class OrdinaryService(int p1, int p2, int p3, int p4); // Noncompliant

public class MarkedSecondaryConstructor(int p1, int p2, int p3, int p4) // Noncompliant
{
    [ActivatorUtilitiesConstructor]
    public MarkedSecondaryConstructor(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8) : this(0, 0, 0, 0) { } // Compliant
}

[Controller]
public partial class PartialController;

public partial class PartialController(int p1, int p2, int p3, int p4); // Compliant

public partial class PartialActivation;

[method: ActivatorUtilitiesConstructor]
public partial class PartialActivation(int p1, int p2, int p3, int p4); // Compliant
