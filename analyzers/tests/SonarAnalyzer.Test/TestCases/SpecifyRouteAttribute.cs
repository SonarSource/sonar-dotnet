using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

public class NoActionHasHttpAttributeWithRouteController : Controller
{
    public IActionResult Index() => View();   // Compliant

    [HttpGet]
    public IActionResult Index2() => View();  // Compliant, default behavior if not route template is defined

    public IActionResult Error() => View();  // Compliant
}

public class ActionHasHttpAttributeWithRouteController : Controller // Noncompliant [controller]
{
    [HttpGet("GetObject")]          
    public IActionResult Get() => View(); // Secondary [controller]

    [HttpPost("CreateObject")]     
    public IActionResult Post() => View(); // Secondary [controller]

    [HttpPut("UpdateObject")]      
    public IActionResult Put() => View(); // Secondary [controller]

    [HttpDelete("DeleteObject")]  
    public IActionResult Delete() => View(); // Secondary [controller]

    [HttpPatch("PatchObject")]    
    public IActionResult Patch() => View(); // Secondary [controller]

    [HttpHead("Head")]           
    public IActionResult HttpHead() => View(); // Secondary [controller]

    [HttpOptions("Options")]     
    public IActionResult HttpOptions() => View(); // Secondary [controller]
}

public class WithUserDefinedAttribute : Controller
{
    [MyHttpMethod("Test")]       // Compliant, behavior is user defined
    public IActionResult Index() => View();

    private sealed class MyHttpMethodAttribute(string template) : HttpMethodAttribute([template]) { }
}
