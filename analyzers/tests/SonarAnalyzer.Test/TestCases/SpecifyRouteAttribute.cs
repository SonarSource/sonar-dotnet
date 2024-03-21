using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using MA = Microsoft.AspNetCore;

public class RouteTemplateIsNotSpecifiedController : Controller
{
    public IActionResult Index() => View();                                     // Compliant

    [HttpGet]
    public IActionResult Index2() => View();                                    // Compliant

    [HttpGet()]
    public IActionResult Index3() => View();                                    // Compliant

    [HttpGetAttribute]
    public IActionResult Index4() => View();                                    // Compliant

    [Microsoft.AspNetCore.Mvc.HttpGet]
    public IActionResult Index5() => View();                                    // Compliant

    [MA.Mvc.HttpGet]
    public IActionResult Index6() => View();                                    // Compliant

    [method: HttpGet]
    public IActionResult Index7() => View();                                    // Compliant

    public IActionResult Error() => View();                                     // Compliant
}

public class RouteTemplatesAreSpecifiedController : Controller                  // Noncompliant [controller] {{Specify the RouteAttribute when an HttpMethodAttribute is specified at an action level.}}
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
{
    private const string ConstantRoute = "ConstantRoute";

    [HttpGet("GetObject")]
    public IActionResult Get() => View();
//                       ^^^ Secondary [controller]

    [HttpGet("GetFirst")]
    [HttpGet("GetSecond")]
    public IActionResult GetMultipleTemplates() => View();                      // Secondary [controller]

    [HttpGet("GetFirst")]
    [HttpPut("PutFirst")]
    public IActionResult Mix() => View();                                       // Secondary [controller]

    [HttpPost("CreateObject")]
    public IActionResult Post() => View();                                      // Secondary [controller]

    [HttpPut("UpdateObject")]
    public IActionResult Put() => View();                                       // Secondary [controller]

    [HttpDelete("DeleteObject")]
    public IActionResult Delete() => View();                                    // Secondary [controller]

    [HttpPatch("PatchObject")]
    public IActionResult Patch() => View();                                     // Secondary [controller]

    [HttpHead("Head")]
    public IActionResult HttpHead() => View();                                  // Secondary [controller]

    [HttpOptions("Options")]
    public IActionResult HttpOptions() => View();                               // Secondary [controller]

    [Route("details")]
    public IActionResult WithRoute() => View();                                 // Secondary [controller]

    [Route("details", Order = 1)]
    public IActionResult WithRouteAndProperties1() => View();                   // Secondary [controller]

    [Route("details", Order = 1, Name = "Details")]
    public IActionResult WithRouteAndProperties2() => View();                   // Secondary [controller]

    [Route("details", Name = "Details", Order = 1)]
    public IActionResult WithRouteAndProperties3() => View();                   // Secondary [controller]

    [Route("[controller]/List/{sortBy}/{direction}")]
    [HttpGet("[controller]/Search/{sortBy}/{direction}")]
    public IActionResult RouteAndMethodMix(string sortBy) => View();            // Secondary [controller]

    [HttpGet("details", Order = 1)]
    public IActionResult MultipleProperties1() => View();                       // Secondary [controller]

    [HttpGet("details", Order = 1, Name = "Details")]
    public IActionResult MultipleProperties2() => View();                       // Secondary [controller]

    [HttpGet("details", Name = "Details", Order = 1)]
    public IActionResult MultipleProperties3() => View();                       // Secondary [controller]

    [HttpGet(ConstantRoute)]
    public IActionResult Constant() => View();                                  // Secondary [controller]

    [HttpGet("""
             ConstantRoute
             """)]
    public IActionResult Constant2() => View();                                 // Secondary [controller]

    [HttpGet($"Route {ConstantRoute}")]
    public IActionResult Interpolation1() => View();                            // Secondary [controller]

    [HttpGet($"""
             {ConstantRoute}
             """)]
    public IActionResult Interpolation2() => View();                            // Secondary [controller]

    [HttpGet("GetObject")]
    public ActionResult WithActionResult() => View();                           // Secondary [controller]

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
public class Endpoint {}
