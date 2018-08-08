namespace Tests.Diagnostics
{
    public class DotNetCoreController : Microsoft.AspNetCore.Mvc.Controller
    {
        public Microsoft.AspNetCore.Mvc.IActionResult Bar(Microsoft.AspNetCore.Routing.IRouter router)
        {
            new Microsoft.AspNetCore.Mvc.VirtualFileResult(@"C:\path\stuff.txt", "text/javascript"); // Noncompliant
            new Microsoft.AspNetCore.Routing.VirtualPathData(router, @"C:\path\stuff.txt"); // Noncompliant

            new Microsoft.AspNetCore.Mvc.VirtualFileResult("/scripts/file.js", "text/javascript");
            new Microsoft.AspNetCore.Routing.VirtualPathData(router, "/my/path");
            return View();
        }
    }
}
