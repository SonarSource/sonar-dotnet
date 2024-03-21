using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using MA = Microsoft.AspNetCore;

public class RouteTemplateIsNotSpecified : Controller
{
    public IActionResult Index() => View();                 // Compliant

    [HttpGet]
    public IActionResult Index2() => View();                // Compliant

    [HttpGet()]
    public IActionResult Index3() => View();                // Compliant

    [HttpGetAttribute]
    public IActionResult Index4() => View();                // Compliant

    [Microsoft.AspNetCore.Mvc.HttpGet]
    public IActionResult Index5() => View();                // Compliant

    [MA.Mvc.HttpGet]
    public IActionResult Index6() => View();                // Compliant

    [method: HttpGet]
    public IActionResult Index7() => View();                // Compliant

    public IActionResult Error() => View();                 // Compliant
}

public class RouteTemplatesAreSpecified : Controller        // Noncompliant [controller] {{Specify the RouteAttribute when an HttpMethodAttribute is specified at an action level.}}
//           ^^^^^^^^^^^^^^^^^^^^^^^^^^
{
    private const string ConstantRoute = "ConstantRoute";

    [HttpGet("GetObject")]
    public IActionResult Get() => View();
//                       ^^^ Secondary [controller]

    [HttpGet("GetFirst")]
    [HttpGet("GetSecond")]
    public IActionResult GetMultipleTemplates() => View();  // Secondary [controller]

    [HttpGet("GetFirst")]
    [HttpPut("GetSecond")]
    public IActionResult Mix() => View();                   // Secondary [controller]

    [HttpPost("CreateObject")]
    public IActionResult Post() => View();                  // Secondary [controller]

    [HttpPut("UpdateObject")]
    public IActionResult Put() => View();                   // Secondary [controller]

    [HttpDelete("DeleteObject")]
    public IActionResult Delete() => View();                // Secondary [controller]

    [HttpPatch("PatchObject")]
    public IActionResult Patch() => View();                 // Secondary [controller]

    [HttpHead("Head")]
    public IActionResult HttpHead() => View();              // Secondary [controller]

    [HttpOptions("Options")]
    public IActionResult HttpOptions() => View();           // Secondary [controller]

    [Route("details")]
    public IActionResult GetDetails() => View();            // FN

    [HttpGet("details", Order = 1)]
    public IActionResult GetDetails2() => View();           // Secondary [controller]

    [HttpGet("details", Order = 1, Name = "Details")]
    public IActionResult GetDetails3() => View();           // Secondary [controller]

    [HttpGet(ConstantRoute)]
    public IActionResult GetDetails4() => View();           // Secondary [controller]

    [HttpGet("""
             ConstantRoute
             """)]
    public IActionResult GetDetails5() => View();           // Secondary [controller]

    [HttpGet($"Route {ConstantRoute}")]
    public IActionResult GetDetails6() => View();           // Secondary [controller]

    [HttpGet($"""
             {ConstantRoute}
             """)]
    public IActionResult GetDetails7() => View();           // Secondary [controller]

    [HttpGet("GetObject")]
    public ActionResult GetDetails8() => View();            // Secondary [controller]

    // [HttpPost("Comment")]
    public IActionResult Comment() => View();
}

public class WithUserDefinedAttribute : Controller          // Noncompliant [customAttribute]
{
    [MyHttpMethod("Test")]
    public IActionResult Index() => View();                 // Secondary [customAttribute]

    private sealed class MyHttpMethodAttribute(string template) : HttpMethodAttribute([template]) { }
}

public class WithCustomGetAttribute : Controller
{
    [HttpGet("Test")]                                       // Compliant, behavior is user-defined
    public IActionResult Index() => View();
    private sealed class HttpGetAttribute(string template) : HttpMethodAttribute([template]) { }
}
