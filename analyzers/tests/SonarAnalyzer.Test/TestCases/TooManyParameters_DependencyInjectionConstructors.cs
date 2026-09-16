using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

public class ActivationConstructor
{
    public ActivationConstructor() { }

    [ActivatorUtilitiesConstructor]
    public ActivationConstructor(int p1, int p2, int p3, int p4) { } // Compliant

    public ActivationConstructor(int p1, int p2, int p3, int p4, int p5) { } // Noncompliant

    public void Method(int p1, int p2, int p3, int p4) { } // Noncompliant
}

public class DerivedFromActivationConstructor : ActivationConstructor
{
    public DerivedFromActivationConstructor(int p1, int p2, int p3, int p4) { } // Noncompliant, constructor attributes are not inherited
}

public struct ActivationStruct
{
    [ActivatorUtilitiesConstructor]
    public ActivationStruct(int p1, int p2, int p3, int p4) { } // Compliant
}

public class ActualController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    public ActualController(int p1, int p2, int p3, int p4) { } // Compliant

    public void Action(int p1, int p2, int p3, int p4) { } // Noncompliant
}

[ApiController]
public class AnnotatedApiController
{
    public AnnotatedApiController() { }
    public AnnotatedApiController(int p1, int p2, int p3, int p4) { } // Compliant

    public void Action(int p1, int p2, int p3, int p4) { } // Noncompliant
}

[Controller]
public class AnnotatedController
{
    public AnnotatedController() { }
    public AnnotatedController(int p1, int p2, int p3, int p4) { } // Compliant

    public void Action(int p1, int p2, int p3, int p4) { } // Noncompliant
}

public class InheritedApiController : AnnotatedApiController
{
    public InheritedApiController(int p1, int p2, int p3, int p4) { } // Compliant
}

public class InheritedController : AnnotatedController
{
    public InheritedController(int p1, int p2, int p3, int p4) { } // Compliant
}

[CustomController]
public class CustomAnnotatedController
{
    public CustomAnnotatedController(int p1, int p2, int p3, int p4) { } // Compliant
}

public class CustomControllerAttribute : ControllerAttribute { }

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class NonInheritedControllerAttribute : ControllerAttribute { }

[NonInheritedController]
public class NonInheritedControllerBase { }

public class DerivedFromNonInheritedController : NonInheritedControllerBase
{
    public DerivedFromNonInheritedController(int p1, int p2, int p3, int p4) { } // Noncompliant
}

[NonController]
public class OptedOutController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    public OptedOutController(int p1, int p2, int p3, int p4) { } // Noncompliant, [NonController] opts the type out of MVC activation
}

[ApiController]
[NonController]
public class OptedOutApiController
{
    public OptedOutApiController(int p1, int p2, int p3, int p4) { } // Noncompliant
}

public class FilterAttribute : ActionFilterAttribute    // ActionFilterAttribute implements IFilterMetadata through IActionFilter
{
    public FilterAttribute(int p1, int p2, int p3, int p4) { } // Noncompliant, the arguments are written at every [Filter(...)] usage site
}

public class ActivatedFilterAttribute : ActionFilterAttribute
{
    [ActivatorUtilitiesConstructor]
    public ActivatedFilterAttribute(int p1, int p2, int p3, int p4) { } // Compliant, explicitly marked as the activation constructor, for use with [TypeFilter]
}

public class ServiceFilter : IActionFilter    // A filter that is not an attribute is resolved from the container
{
    public ServiceFilter(int p1, int p2, int p3, int p4) { } // Compliant

    public void OnActionExecuting(ActionExecutingContext context) { }
    public void OnActionExecuted(ActionExecutedContext context) { }
}

public class OrdinaryService
{
    public OrdinaryService(int p1, int p2, int p3, int p4) { } // Noncompliant
}

[Other.ApiController, Other.Controller]
public class UnrelatedAttributes
{
    [Other.ActivatorUtilitiesConstructor]
    public UnrelatedAttributes(int p1, int p2, int p3, int p4) { } // Noncompliant
}

public class UnrelatedBaseType : Other.Controller
{
    public UnrelatedBaseType(int p1, int p2, int p3, int p4) { } // Noncompliant
}

public class UnrelatedInterface : Other.IHostedService
{
    public UnrelatedInterface(int p1, int p2, int p3, int p4) { } // Noncompliant
}

namespace Other
{
    public class ApiControllerAttribute : Attribute { }
    public class ControllerAttribute : Attribute { }
    public class ActivatorUtilitiesConstructorAttribute : Attribute { }
    public class Controller { }
    public interface IHostedService { }
}
