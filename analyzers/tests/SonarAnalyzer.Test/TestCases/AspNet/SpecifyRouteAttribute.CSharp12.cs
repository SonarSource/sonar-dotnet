using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using MA = Microsoft.AspNetCore;

public class RouteTemplateIsNotSpecifiedController : Controller
{
    public IActionResult NoAttribute() => View();                               // Compliant

    [HttpGet]
    public IActionResult WithHttpGetAttribute() => View();                      // Compliant

    [HttpGet()]
    public IActionResult WithHttpGetAttributeWithParanthesis() => View();       // Compliant

    [HttpGetAttribute]
    public IActionResult WithFullAttributeName() => View();                     // Compliant

    [Microsoft.AspNetCore.Mvc.HttpGet]
    public IActionResult WithNamespaceAttribute() => View();                    // Compliant

    [MA.Mvc.HttpGet]
    public IActionResult WithAliasedNamespaceAttribute() => View();             // Compliant

    [method: HttpGet]
    public IActionResult WithScopedAttribute() => View();                       // Compliant

    [HttpGet("/[controller]/[action]/{sortBy}")]
    public IActionResult AbsoluteUri1(string sortBy) => View();                 // Compliant, absolute uri

    [HttpGet("~/[controller]/[action]/{sortBy}")]
    public IActionResult AbsoluteUri2(string sortBy) => View();                 // Compliant, absolute uri

    public IActionResult Error() => View();                                     // Compliant
}

public class RouteTemplatesAreSpecifiedController : Controller                  // Noncompliant [controller] {{Specify the RouteAttribute when an HttpMethodAttribute or RouteAttribute is specified at an action level.}}
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
{
    private const string ConstantRoute = "ConstantRoute";

    [HttpGet("GetObject")]
    public IActionResult Get() => View();
//                       ^^^ Secondary [controller]

    [HttpGet("GetFirst")]
    [HttpGet("GetSecond")]
    public IActionResult GetMultipleTemplates() => View();                      // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet("GetFirst")]
    [HttpPut("PutFirst")]
    public IActionResult MixGetAndPut() => View();                              // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet("GetFirst")]
    [HttpPut()]
    public IActionResult MixWithTemplateAndWithout() => View();                 // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet()]
    [HttpPut()]
    public IActionResult MixWithoutTemplate() => View();

    [HttpPost("CreateObject")]
    public IActionResult Post() => View();                                      // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpPut("UpdateObject")]
    public IActionResult Put() => View();                                       // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpDelete("DeleteObject")]
    public IActionResult Delete() => View();                                    // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpPatch("PatchObject")]
    public IActionResult Patch() => View();                                     // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpHead("Head")]
    public IActionResult HttpHead() => View();                                  // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpOptions("Options")]
    public IActionResult HttpOptions() => View();                               // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [Route("details")]
    public IActionResult WithRoute() => View();                                 // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [Route("details", Order = 1)]
    public IActionResult WithRouteAndProperties1() => View();                   // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [Route("details", Order = 1, Name = "Details")]
    public IActionResult WithRouteAndProperties2() => View();                   // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [Route("details", Name = "Details", Order = 1)]
    public IActionResult WithRouteAndProperties3() => View();                   // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [Route("[controller]/List/{sortBy}/{direction}")]
    [HttpGet("[controller]/Search/{sortBy}/{direction}")]
    public IActionResult RouteAndMethodMix(string sortBy) => View();            // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet("details", Order = 1)]
    public IActionResult MultipleProperties1() => View();                       // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet("details", Order = 1, Name = "Details")]
    public IActionResult MultipleProperties2() => View();                       // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet("details", Name = "Details", Order = 1)]
    public IActionResult MultipleProperties3() => View();                       // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet(ConstantRoute)]
    public IActionResult Constant() => View();                                  // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet("""
             ConstantRoute
             """)]
    public IActionResult Constant2() => View();                                 // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet($"Route {ConstantRoute}")]
    public IActionResult Interpolation1() => View();                            // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet($"""
             {ConstantRoute}
             """)]
    public IActionResult Interpolation2() => View();                            // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [HttpGet("GetObject")]
    public ActionResult WithActionResult() => View();                           // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [Route(" ")]
    public IActionResult WithSpace() => View();                                 // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    [Route("\t")]
    public IActionResult WithTab() => View();                                   // Secondary [controller] {{By specifying an HttpMethodAttribute or RouteAttribute here, you will need to specify the RouteAttribute at the class level.}}

    // [HttpPost("Comment")]
    public IActionResult Comment() => View();
}

[Route("api/[controller]")]
public class WithRouteAttributeIsCompliantController : Controller
{
    [HttpGet("Test")]
    public IActionResult Index() => View();
}

public class WithUserDefinedAttributeController : Controller                    // Noncompliant [customAttribute]
{
    [MyHttpMethod("Test")]
    public IActionResult Index() => View();                                     // Secondary [customAttribute]

    private sealed class MyHttpMethodAttribute(string template) : HttpMethodAttribute([template]) { }
}

public class WithCustomGetAttributeController : Controller                      // Noncompliant [custom-get-attribute]
{
    [HttpGet("Test")]
    public IActionResult Index() => View();                                     // Secondary [custom-get-attribute]

    private sealed class HttpGetAttribute(string template) : HttpMethodAttribute([template]) { }
}

public class WithCustomController : DerivedController                           // Noncompliant [derivedController]
{
    [HttpGet("Test")]
    public IActionResult Index() => View();                                     // Secondary [derivedController]
}

[Controller]
public class WithAttributeController                                            // Noncompliant [attribute-controller]
{
    [HttpGet("Test")]
    public string Index() => "Hi!";                                             // Secondary [attribute-controller]
}

public class WithAttributeControllerUsingInheritanceController : Endpoint       // FN
{
    [HttpGet("Test")]
    public string Index() => "Hi!";                                             // FN
}

public class NamedController                                                    // FN
{
    [HttpGet("Test")]
    public string Index() => "Hi!";                                             // FN
}

[NonController]
public class NonController
{
    [HttpGet("Test")]
    public string Index() => "Hi!";
}
public class DerivedController : Controller { }

[Controller]
public class Endpoint { }

[Route("api/[controller]")]
public class ControllerWithRouteAttribute : Controller { }

public class ControllerWithInheritedRoute : ControllerWithRouteAttribute            // Compliant, attribute is inherited
{
    [HttpGet("Test")]                                                               // Route: api/ControllerWithInheritedRoute/Test
    public string Index() => "Hi!";
}

public class BaseControllerWithActionWithRoute : Controller                         // Noncompliant
{
    [HttpGet("Test")]                                                               //Route: /Test (AmbiguousMatchException raised because of the override in ControllerOverridesActionWithRoute)
    public virtual string Index() => "Hi!";                                         // Secondary

    // Route: BaseControllerWithActionWithRoute/Index/1
    public virtual string Index(int id) => "Hi!";                                   // Compliant
}

public class ControllerOverridesActionWithRoute : BaseControllerWithActionWithRoute // Noncompliant
{
    // Route: /Test (AmbiguousMatchException raised because the base method is also in scope)
    public override string Index() => "Hi!";                                        // Secondary

    // Route: ControllerOverridesActionWithRoute/Index/1
    public override string Index(int id) => "Hi!";                                  // Compliant
}

// Repro: https://github.com/SonarSource/sonar-dotnet/issues/8985
public sealed class ExtendedRouteAttribute() : RouteAttribute("[controller]/[action]");

[ExtendedRoute]
public class SomeController : ControllerBase    // Compliant - the route attribute template is set in the base class of ExtendedRouteAttribute
{
    [HttpGet("foo")]
    public string Foo() => "Hi";
}

// https://github.com/SonarSource/sonar-dotnet/issues/9252
namespace AbstractControllerClass
{
    public abstract class BaseController : Controller   // Compliant - class is abstract
    {
        [HttpGet]
        [Route("list")]
        public IActionResult List()
        {
            // ... load and return items
            return View();
        }
    }

    [Route("/api/user")]
    public sealed class UserController : BaseController
    {
        // other controller code
    }
}
