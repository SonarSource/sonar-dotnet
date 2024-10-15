using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Diagnostics.CodeAnalysis;

class ControllerRequirementsDontInfluenceRouteCheck
{
    [NonController]
    [Route(@"A\[controller]")]    // Noncompliant
    public class NotAController : Controller { }

    [Route(@"A\[controller]")]    // Noncompliant
    public class ControllerWithoutControllerSuffix : Controller { }

    [Controller]
    [Route(@"A\[controller]")]    // Noncompliant
    public class ControllerWithControllerAttribute : Controller { }

    [Route(@"A\[controller]")]    // Noncompliant
    internal class InternalController : Controller { }

    [Route(@"A\[controller]")]    // Noncompliant
    protected class ProtectedController : Controller { }

    [Route(@"A\[controller]")]    // Noncompliant
    private protected class PrivateProtectedController : Controller { }

    [Route(@"A\[controller]")]    // Noncompliant
    public class  ControllerWithoutParameterlessConstructor : Controller
    {
        public ControllerWithoutParameterlessConstructor(int i) { }
    }
    [Route(@"A\e\[controller]")]    // Noncompliant
    public class ControllerWithEscapeChar : Controller { }
}
