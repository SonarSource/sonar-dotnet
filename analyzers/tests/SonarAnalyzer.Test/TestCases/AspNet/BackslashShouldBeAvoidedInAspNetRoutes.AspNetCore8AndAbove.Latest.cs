using Microsoft.AspNetCore.Builder;

class WithAllControllerEndpointRouteBuilderExtensionsMethods
{
    // MapFallbackToPage decorates the pattern with StringSyntax("Route") since ASP.NET Core 8.0.0
    void MapFallbackToPage(WebApplication app)
    {
        app.MapFallbackToPage("\\action");                                    // Compliant
        app.MapFallbackToPage("{controller=Home}\\{action=Index}", "\\page"); // Noncompliant
        //                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        app.MapFallbackToPage("\eaction");                                    // Compliant
        app.MapFallbackToPage("{controller=Home}\\\e{action=Index}", "\e\\page"); // Noncompliant

    }
}
