using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

public class WithUserDefinedAttributeDerivedFromHttpMethodAttributeController : Controller // Noncompliant: MyHttpMethodAttribute derives from HttpMethodAttribute
{
    [MyHttpMethod("/Index")] // Secondary
    public IActionResult WithUserDefinedAttribute() => View();

    private sealed class MyHttpMethodAttribute(string template) : HttpMethodAttribute([template]) { }
}
