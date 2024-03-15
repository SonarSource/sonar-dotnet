using System.Web.Mvc;

[Route("[controller]")]
public class NoncompliantController : Controller // Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
//           ^^^^^^^^^^^^^^^^^^^^^^
{
    [Route("/Index1")]                  // Secondary
//   ^^^^^^^^^^^^^^^^
    public ActionResult Index1() => View();

    [Route("/SubPath/Index2_1")]        // Secondary
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^
    [Route("/[controller]/Index2_2")]   // Secondary
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public ActionResult Index2() => View();

    [Route("/[action]")]                // Secondary
//   ^^^^^^^^^^^^^^^^^^
    public ActionResult Index3() => View();

    [Route("/SubPath/Index4_1")]        // Secondary
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^
    [Route("/[controller]/Index4_2")]   // Secondary
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public ActionResult Index4() => View();
}

[RoutePrefix("[controller]")]
public class NoncompliantWithRoutePrefixController : Controller // Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
{
    [Route("/Index1")]                  // Secondary
//   ^^^^^^^^^^^^^^^^
    public ActionResult Index1() => View();
}

[Route("[controller]")]
[Route("[controller]/[action]")]
public class NoncompliantMultiRouteController : Controller // Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
{
    [Route("/Index1")]                  // Secondary
//   ^^^^^^^^^^^^^^^^
    public ActionResult Index1() => View();

    [Route("/SubPath/Index2_1")]        // Secondary
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^
    [Route("/[controller]/Index2_2")]   // Secondary
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public ActionResult Index2() => View();

    [Route("/[action]")]                // Secondary
//   ^^^^^^^^^^^^^^^^^^
    public ActionResult Index3() => View();
}

[Route("[controller]")]
public class CompliantController : Controller // Compliant: at least one action has at least a relative route
{
    [Route("/Index1")]
    public ActionResult Index1() => View();

    [Route("/SubPath/Index2")]
    public ActionResult Index2() => View();

    [Route("/[action]")]
    public ActionResult Index3() => View();

    [Route("/[controller]/Index4_1")]
    [Route("SubPath/Index4_2")] // The relative route
    public ActionResult Index4() => View();
}

public class NoncompliantNoControllerRouteController : Controller // Noncompliant {{Change the paths of the actions of this controller to be relative and add a controller route with the common prefix.}}
{
    [Route("/Index1")]                          // Secondary
    public ActionResult Index1() => View();

    [Route("/SubPath/Index2_1")]                // Secondary
    [Route("/[controller]/Index2_2")]           // Secondary
    public ActionResult Index2() => View();

    [Route("/[action]")]                        // Secondary
    public ActionResult Index3() => View();
}

public class CompliantNoControllerRouteNoActionRouteController : Controller // Compliant
{
    public ActionResult Index1() => View(); // Default route -> relative

    [Route("/SubPath/Index2")]
    public ActionResult Index2() => View();

    [Route("/[action]")]
    public ActionResult Index3() => View();

    [Route("/SubPath/Index4_1")]
    [Route("/[controller]/Index4_2")]
    public ActionResult Index4() => View();
}

public class CompliantNoControllerRouteEmptyActionRouteController : Controller // Compliant
{
    [Route]
    public ActionResult Index1() => View(); // Empty route -> relative

    [Route("/SubPath/Index2")]
    public ActionResult Index2() => View();

    [Route("/[action]")]
    public ActionResult Index3() => View();

    [Route("/SubPath/Index4_1")]
    [Route("/[controller]/Index4_2")]
    public ActionResult Index4() => View();
}

public class EmptyController : Controller { } // Compliant

namespace WithAliases
{
    using MyRoute = RouteAttribute;
    using ASP = System.Web;

    public class WithAliasedRouteAttributeController : Controller // Noncompliant
    {
        [MyRoute(@"/[controller]")] // Secondary
        public ActionResult Index() => View();
    }

    public class WithFullQualifiedPartiallyAliasedNameController : Controller // Noncompliant
    {
        [ASP.Mvc.RouteAttribute("/[action]")] // Secondary
        public ActionResult Index() => View();
    }
}
