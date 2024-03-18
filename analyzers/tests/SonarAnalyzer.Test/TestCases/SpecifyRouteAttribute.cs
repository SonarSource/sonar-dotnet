using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

public class NoActionHasHttpAttributeWithRouteController : Controller
{
    public IActionResult Index() => View();   // Compliant

    [HttpGet]
    public IActionResult Index2() => View();  // Compliant, default behavior if not route template is defined

    public IActionResult Error() => View();  // Compliant
}

public class ActionHasHttpAttributeWithRouteController : Controller
{
    [HttpGet("GetObject")]          // Noncompliant
    public IActionResult Get() => View();

    [HttpPost("CreateObject")]     // Noncompliant
    public IActionResult Post() => View();

    [HttpPut("UpdateObject")]      // Noncompliant
    public IActionResult Put() => View();

    [HttpDelete("DeleteObject")]  // Noncompliant
    public IActionResult Delete() => View();

    [HttpPatch("PatchObject")]    // Noncompliant
    public IActionResult Patch() => View();

    [HttpHead("Head")]           // Noncompliant
    public IActionResult HttpHead() => View();

    [HttpOptions("Options")]     // Noncompliant
    public IActionResult HttpOptions() => View();
}

public class WithUserDefinedAttribute : Controller
{
    [MyHttpMethod("Test")]       // Compliant, behavior is user defined
    public IActionResult Index() => View();

    private sealed class MyHttpMethodAttribute(string template) : HttpMethodAttribute([template]) { }
}
