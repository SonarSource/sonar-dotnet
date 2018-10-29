using Microsoft.AspNetCore.Mvc;

namespace Tests.Diagnostics
{
    public class DotNetCoreController : Controller
    {
        public IActionResult Bar(Microsoft.AspNetCore.Routing.IRouter router)
        {
            new Microsoft.AspNetCore.Mvc.VirtualFileResult(@"C:\path\stuff.txt", "text/javascript"); // Noncompliant
            new Microsoft.AspNetCore.Routing.VirtualPathData(router, @"C:\path\stuff.txt"); // Noncompliant

            new Microsoft.AspNetCore.Mvc.VirtualFileResult("/scripts/file.js", "text/javascript");
            new Microsoft.AspNetCore.Routing.VirtualPathData(router, "/my/path");
            return View();
        }
    }
}
