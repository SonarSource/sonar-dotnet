using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

class WithAllControllerEndpointRouteBuilderExtensionsMethods
{
    private const string ASlash = "/";
    private const string ABackslash = @"\";

    void MapControllerRoute(WebApplication app)
    {
        const string ASlashLocal = "A";
        const string ABackslashLocal = @"\";

        app.MapControllerRoute("default", "{controller=Home}\\{action=Index}/{id?}");                  // Noncompliant
        app.MapControllerRoute("default", @"{controller=Home}\\{action=Index}/{id?}");                 // Noncompliant
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");                   // Compliant

        app.MapControllerRoute("default", $$"""{controller=Home}{{ABackslash}}{action=Index}""");      // Noncompliant
        app.MapControllerRoute("default", $$"""{controller=Home}{{ASlash}}{action=Index}""");          // Compliant
        app.MapControllerRoute("default", $$"""{controller=Home}{{ABackslashLocal}}{action=Index}"""); // Noncompliant
        app.MapControllerRoute("default", $$"""{controller=Home}{{ASlashLocal}}{action=Index}""");     // Compliant

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}\\{action=Index}/{id?}", // Noncompliant
            defaults: new { controller = "Home", action = "Index" });
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}\e\\{action=Index}/{id?}", // Noncompliant
            defaults: new { controller = "Home", action = "Index" });
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}\e{action=Index}/{id?}", // Compliant
            defaults: new { controller = "Home", action = "Index" });
        app.MapControllerRoute(
            pattern: "{controller=Home}\\{action=Index}/{id?}", // Noncompliant
            name: "default",
            defaults: new { controller = "Home", action = "Index" });

        ControllerEndpointRouteBuilderExtensions.MapControllerRoute(app, "default", "{controller=Home}\\{action=Index}/{id?}"); // Noncompliant
    }

    void OtherMethods(WebApplication app)
    {
        app.MapAreaControllerRoute("default", "area", "{controller=Home}\\{action=Index}/{id?}");             // Noncompliant
        //                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        app.MapFallbackToAreaController("{controller=Home}\\{action=Index}", "action", "controller", "area"); // Noncompliant
        app.MapFallbackToAreaController("\\action", "\\controller", "\\area");                                // Compliant
        app.MapFallbackToController(@"\action", @"\controller");                                              // Compliant
        app.MapFallbackToController("{controller=Home}\\{action=Index}", "\\action", "\\controller");         // Noncompliant
        //                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        app.MapDynamicControllerRoute<ATransformer>("{controller=Home}\\{action=Index}");                     // Noncompliant
        app.MapDynamicControllerRoute<ATransformer>("{controller=Home}\\{action=Index}", new object());       // Noncompliant
        app.MapDynamicControllerRoute<ATransformer>("{controller=Home}\\{action=Index}", new object(), 3);    // Noncompliant
    }

    private sealed class ATransformer : DynamicRouteValueTransformer
    {
        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values) => throw new NotImplementedException();
    }
}
