using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using static System.Web.HttpContext;

namespace Tests.Diagnostics
{
    class Program
    {
        void InvalidCases(string s1, string s2)
        {
            var fileLiteral = "file://blah.txt"; // Noncompliant {{Refactor your code not to use hardcoded absolute paths or URIs.}}
//                            ^^^^^^^^^^^^^^^^^

            var webUri1 = "http://www.mywebsite.com"; // Noncompliant
            var webUri2 = "https://www.mywebsite.com"; // Noncompliant
            var webUri3 = "ftp://www.mywebsite.com"; // Noncompliant

            var windowsDrivePath1 = "c:\\blah\\blah\\blah.txt"; // Noncompliant
            var windowsDrivePath2 = "C:\\blah\\blah\\blah.txt"; // Noncompliant
            var windowsDrivePath3 = "C:/blah/blah/blah.txt"; // Noncompliant
            var windowsDrivePath4 = @"C:\blah\blah\blah.txt"; // Noncompliant
            var windowsDrivePath5 = @"C:\%foo%\Documents and Settings\file.txt"; // Noncompliant

            var windowsSharedDrivePath1 = @"\\my-network-drive\folder\file.txt"; // Noncompliant
            var windowsSharedDrivePath2 = @"\\my-network-drive\Documents and Settings\file.txt"; // Noncompliant
            var windowsSharedDrivePath3 = "\\\\my-network-drive\\folder\\file.txt"; // Noncompliant
            var windowsSharedDrivePath4 = @"\\my-network-drive\%foo%\file.txt"; // Noncompliant
            var windowsSharedDrivePath5 = @"\\my-network-drive/folder/file.txt"; // Noncompliant

            var unixPath1 = "/my/other/folder"; // Noncompliant
            var unixPath2 = "~/blah/blah/blah.txt"; // Noncompliant
            var unixPath3 = "~\\blah\\blah\\blah.txt"; // Noncompliant

            var concatWithDelimiterPath1 = s1 + "\\" + s2; // Noncompliant {{Remove this hardcoded path-delimiter.}}
//                                              ^^^^
            var concatWithDelimiterUri2 = s1 + @"\" + s2; // Noncompliant
            var concatWithDelimiterUri3 = s1 + "/" + s2; // Noncompliant

            var x = new Uri("C:/test.txt"); // Noncompliant
            new Uri(new Uri("../stuff"), ("C:/test.txt")); // Noncompliant
            File.OpenRead(@"\\drive\foo.csv"); // Noncompliant

            var unixChemin = "/my/other/folder"; // FN
            var webChemin = "http://www.mywebsite.com"; // FN
            var windowsChemin = "c:\\blah\\blah\\blah.txt"; // FN
        }

        void ValidCases(string s)
        {
            var windowsPathStartingWithVariable = "%AppData%\\Adobe";
            var windowsPathWithVariable = "%appdata%";

            var relativePath1 = "./my/folder";
            var relativePath2 = @".\my\folder";
            var relativePath3 = @"..\..\Documents";
            var relativePath4 = @"../../Documents";
            var file = "file.txt";

            var driveLetterPath = "C:";

            var concat1 = s + "\\" + s;
        }
    }

    #region virtual-path-tests

    public class AspDotNetController : Controller
    {
        public ActionResult Foo()
        {
            string virtualPath = "~/scripts/relative.js"; // Noncompliant FP
            Server.MapPath(virtualPath);

            // System.Web.Mvc.HttpServerUtilityBase
            Server.MapPath("~/scripts/relative.js");
            Server.MapPath("/scripts/absolute.js");
            Server.MapPath(@"C:\path\stuff.txt"); // Noncompliant (n.b. method will throw exception)

            // System.Web.Mvc.HttpRequestBase
            Request.MapPath("~/scripts/relative.js");
            Request.MapPath("/scripts/absolute.jss");
            Request.MapPath(@"C:\path\stuff.txt"); // Noncompliant (n.b. method will throw exception)

            // System.Web.Mvc.HttpResponseBase
            Response.ApplyAppPathModifier("~/scripts/relative.js");
            Response.ApplyAppPathModifier("/scripts/absolute.js");
            Response.ApplyAppPathModifier(@"C:\path\stuff.txt"); // Noncompliant

            // System.Web.VirtualPathUtility (all methods)
            VirtualPathUtility.GetDirectory("~/scripts/relative.js");
            VirtualPathUtility.IsAppRelative("/scripts/absolute.js");
            VirtualPathUtility.GetExtension("/scripts/absolute.js");
            // ...
            VirtualPathUtility.GetFileName(@"C:\path\stuff.txt"); // Noncompliant (n.b. method will throw exception)
            VirtualPathUtility.Combine("root", @"C:\path\stuff.txt"); // Noncompliant
            // ...

            // System.Web.Mvc.UrlHelper
            UrlHelper urlHelper = new UrlHelper(Current.Request.RequestContext);
            urlHelper.Content("~/scripts/relative.js");
            urlHelper.Content("/scripts/absolute.js");
            urlHelper.Content(@"C:\path\stuff.txt"); // Noncompliant

            return View();
        }
    }

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

    #endregion
}
