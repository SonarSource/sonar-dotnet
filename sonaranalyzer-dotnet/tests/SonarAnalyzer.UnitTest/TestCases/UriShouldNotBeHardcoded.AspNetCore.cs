using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

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

        public void Foo(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapAreaRoute("/route/x", "area", "~/route/y");
            System.Console.WriteLine("/unix/path"); // Compliant - we ignore unix paths
        }

        public void Paths()
        {
            new System.IO.FileStream("/unix/path", FileMode.CreateNew); // FN
            System.IO.File.Create("~/unix/path"); // FN
            System.IO.Directory.Delete(@"C:\path\"); // Noncompliant
        }
    }
}
