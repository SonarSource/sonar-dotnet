using System.Web;
using System.Web.Mvc;
using static System.Web.HttpContext;

namespace Tests.Diagnostics
{
    public class AspDotNetController : Controller
    {
        public ActionResult Foo()
        {
            string virtualPath = "~/scripts/relative.js"; // Compliant
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

        public void Bar()
        {
            var scriptPath1 = "~/bundles/jquery"; // should not raise
            var scriptPath2 = "~/Scripts/jquery-{version}.js"; // should not raise
        }
    }
}
