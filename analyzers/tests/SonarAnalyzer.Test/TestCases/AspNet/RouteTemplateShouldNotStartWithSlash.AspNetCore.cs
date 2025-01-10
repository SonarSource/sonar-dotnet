using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

[Route("[controller]")]
public class NoncompliantController : Controller // Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
//           ^^^^^^^^^^^^^^^^^^^^^^
{
    [Route("/Index1")]                  // Secondary {{Change this path to be relative to the controller route defined on class level.}}
//   ^^^^^^^^^^^^^^^^
    public IActionResult Index1() => View();

    [Route("/SubPath/Index2")]          // Secondary {{Change this path to be relative to the controller route defined on class level.}}
//   ^^^^^^^^^^^^^^^^^^^^^^^^
    public IActionResult Index2() => View();

    [HttpGet("/[action]")]              // Secondary {{Change this path to be relative to the controller route defined on class level.}}
//   ^^^^^^^^^^^^^^^^^^^^
    public IActionResult Index3() => View();

    [HttpGet("/SubPath/Index4_1")]      // Secondary {{Change this path to be relative to the controller route defined on class level.}}
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    [HttpGet("/[controller]/Index4_2")] // Secondary {{Change this path to be relative to the controller route defined on class level.}}
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public IActionResult Index4() => View();
}

[Route("[controller]")]
[Route("[controller]/[action]")]
public class NoncompliantMultiRouteController : Controller // Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
{
    [Route("/Index1")]                  // Secondary
//   ^^^^^^^^^^^^^^^^
    public IActionResult Index1() => View();

    [Route("/SubPath/Index2")]          // Secondary
//   ^^^^^^^^^^^^^^^^^^^^^^^^
    public IActionResult Index2() => View();

    [HttpGet("/[action]")]              // Secondary
//   ^^^^^^^^^^^^^^^^^^^^
    public IActionResult Index3() => View();

    [HttpGet("/SubPath/Index4_1")]      // Secondary
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    [HttpGet("/[controller]/Index4_2")] // Secondary
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public IActionResult Index4() => View();
}

[Route("[controller]")]
public class CompliantController : Controller // Compliant: at least one action has at least a relative route
{
    [Route("/Index1")]
    public IActionResult Index1() => View();

    [Route("/SubPath/Index2")]
    public IActionResult Index2() => View();

    [HttpGet("/[action]")]
    public IActionResult Index3() => View();

    [HttpGet("/[controller]/Index4_1")]
    [HttpGet("SubPath/Index4_2")] // The relative route
    public IActionResult Index4() => View();
}

public class NoncompliantNoControllerRouteController : Controller // Noncompliant {{Change the paths of the actions of this controller to be relative and add a controller route with the common prefix.}}
{
    [Route("/Index1")]                          // Secondary {{Add a controller route with a common prefix and change this path to be relative it.}}
    public IActionResult Index1() => View();

    [Route("/SubPath/Index2")]                  // Secondary {{Add a controller route with a common prefix and change this path to be relative it.}}
    public IActionResult Index2() => View();

    [HttpGet("/[action]")]                      // Secondary {{Add a controller route with a common prefix and change this path to be relative it.}}
    public IActionResult Index3() => View();

    [HttpGet("/SubPath/Index4_1")]              // Secondary {{Add a controller route with a common prefix and change this path to be relative it.}}
    [HttpGet("/[controller]/Index4_2")]         // Secondary {{Add a controller route with a common prefix and change this path to be relative it.}}
    public IActionResult Index4() => View();
}

public class CompliantNoControllerRouteNoActionRouteController : Controller // Compliant
{
    public IActionResult Index1() => View(); // Conventional route -> relative

    [Route("/SubPath/Index2")]
    public IActionResult Index2() => View();

    [HttpGet("/[action]")]
    public IActionResult Index3() => View();

    [HttpGet("/SubPath/Index4_1")]
    [HttpGet("/[controller]/Index4_2")]
    public IActionResult Index4() => View();
}

public class CompliantNoControllerRouteEmptyActionRouteController : Controller // Compliant
{
    [HttpGet]
    public IActionResult Index1() => View(); // Empty route template -> relative conventional routing

    [Route("/SubPath/Index2")]
    public IActionResult Index2() => View();

    [HttpGet("/[action]")]
    public IActionResult Index3() => View();

    [HttpGet("/SubPath/Index4_1")]
    [HttpGet("/[controller]/Index4_2")]
    public IActionResult Index4() => View();
}

namespace WithAliases
{
    using MyRoute = RouteAttribute;
    using ASP = Microsoft.AspNetCore;

    public class WithAliasedRouteAttributeController : Controller // Noncompliant
    {
        [MyRoute(@"/[controller]")] // Secondary
        public IActionResult Index() => View();
    }

    public class WithFullQualifiedPartiallyAliasedNameController : Controller // Noncompliant
    {
        [ASP.Mvc.RouteAttribute("/[action]")] // Secondary
        public IActionResult Index() => View();
    }
}

public class MultipleActionsAllRoutesStartingWithSlash1Controller : Controller  // Noncompliant
{
    [HttpGet("/Index1")] // Secondary
    public IActionResult WithHttpAttribute() => View();

    [Route("/Index2")]   // Secondary
    public IActionResult WithRouteAttribute() => View();
}

public class MultipleActionsAllRoutesStartingWithSlash2Controller : Controller  // Noncompliant
{
    [HttpGet("/Index1")] // Secondary
    [HttpGet("/Index3")] // Secondary
    public IActionResult WithHttpAttributes() => View();

    [Route("/Index2")]   // Secondary
    [Route("/Index4")]   // Secondary
    [HttpGet("/Index5")] // Secondary
    public IActionResult WithRouteAndHttpAttributes() => View();
}

[Route("[controller]")]
public class MultipleActionsAllRoutesStartingWithSlash3Controller : Controller  // Noncompliant
{
    [HttpGet("/Index1")] // Secondary
    [HttpGet("/Index3")] // Secondary
    public IActionResult WithHttpAttributes() => View();

    [Route("/Index2")]   // Secondary
    [Route("/Index4")]   // Secondary
    [HttpGet("/Index5")] // Secondary
    public IActionResult WithRouteAndHttpAttributes() => View();
}

public class MultipleActionsSomeRoutesStartingWithSlash1Controller : Controller // Compliant: some routes are relative
{
    [HttpGet("Index1")]
    public IActionResult WithHttpAttribute() => View();

    [Route("/Index2")]
    public IActionResult WithRouteAttribute() => View();
}

public class MultipleActionsSomeRoutesStartingWithSlash2Controller : Controller // Compliant: some routes are relative
{
    [HttpGet("Index1")]
    [HttpGet("/Index1")]
    public IActionResult WithHttpAttributes() => View();

    [Route("/Index2")]
    public IActionResult WithRouteAttribute() => View();
}

public class MultipleActionsSomeRoutesStartingWithSlash3Controller : Controller // Compliant: some routes are relative
{
    [HttpGet("Index1")]
    [HttpPost("/Index1")]
    public IActionResult WithHttpAttributes() => View();

    [Route("/Index2")]
    public IActionResult WithRouteAttribute() => View();
}

[NonController]
public class NotAController : Controller                    // Compliant: not a controller
{
    [Route("/Index1")]
    public IActionResult Index() => View();
}

public class ControllerWithoutControllerSuffix : Controller // Noncompliant
{
    [Route("/Index1")] // Secondary
    public IActionResult Index() => View();
}

[Controller]
public class ControllerWithControllerAttribute : Controller // Noncompliant
{
    [Route("/Index1")] // Secondary
    public IActionResult Index() => View();
}

public class ControllerWithoutParameterlessConstructor : Controller // Noncompliant
{
    public ControllerWithoutParameterlessConstructor(int i) { }

    [Route("/Index1")] // Secondary
    public IActionResult Index() => View();
}

class NonPublicController : Controller                         // Compliant, actions in non-public classes are not reachable
{
    [Route("/Index1")]
    public IActionResult Index() => View();
}


public class ControllerRequirementsInfluenceActionsCheck
{
    internal class InternalController : Controller              // Compliant, nested classes are not reachable
    {
        [Route("/Index1")]
        public IActionResult Index() => View();
    }

    protected class ProtectedController : Controller            // Compliant, nested classes are not reachable
    {
        [Route("/Index1")]
        public IActionResult Index() => View();
    }

    public class PublicNestedController : Controller           // Compliant, actions in nested classes are not reachable
    {
        [Route("/Index1")]
        public IActionResult Index() => View();
    }
}

public struct AStruct
{
    public class PublicNestedController : Controller           // Compliant, actions in nested types are not reachable
    {
        [Route("/Index1")]
        public IActionResult Index() => View();
    }
}

[Route("[controller]")]
public partial class NoncompliantPartialController : Controller // Noncompliant [first]
{
    [Route("/Index1")]                  // Secondary [first, second]
    public IActionResult Index1() => View();

    [Route("/SubPath/Index2")]          // Secondary [first, second]
    public IActionResult Index2() => View();
}

[Route("[controller]")]
public partial class NoncompliantPartialController : Controller // Noncompliant [second]
{
    [HttpGet("/[action]")]              // Secondary [first, second]
    public IActionResult Index3() => View();

    [HttpGet("/SubPath/Index4_1")]      // Secondary [first, second]
    public IActionResult Index4() => View();
}

[Route("[controller]")]
public partial class CompliantPartialController : Controller // Compliant, due to the other partial part of this class
{
    [Route("/Index1")]
    public IActionResult Index1() => View();

    [Route("/SubPath/Index2")]
    public IActionResult Index2() => View();
}

[Route("[controller]")]
public partial class CompliantPartialController : Controller
{
    [HttpGet("[action]")]
    public IActionResult Index3() => View();
}

[Route("[controller]")]
public partial class NoncompliantPartialAutogeneratedController : Controller // Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
{
    [Route("/Index1")]                  // Secondary
    public IActionResult Index1() => View();

    [Route("/SubPath/Index2")]          // Secondary
    public IActionResult Index2() => View();
}

[Route("[controller]")]
public partial class CompliantPartialAutogeneratedController : Controller // Compliant, as its autogenerated partial class is compliant
{
    [Route("/Index1")]
    public IActionResult Index1() => View();

    [Route("/SubPath/Index2")]
    public IActionResult Index2() => View();
}

// https://github.com/SonarSource/sonar-dotnet/issues/9002
public class Repro_9002 : Controller            // Noncompliant
{
    [Route("~/B")]                              // Secondary
    public IActionResult Index() => View();
}
