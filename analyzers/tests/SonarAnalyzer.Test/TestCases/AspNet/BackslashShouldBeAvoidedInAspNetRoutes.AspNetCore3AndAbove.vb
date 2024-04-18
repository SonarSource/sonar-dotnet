Imports Microsoft.AspNetCore.Builder

Class WithAllControllerEndpointRouteBuilderExtensionsMethods
    Private Sub MapControllerRoute(ByVal app As WebApplication)
        app.MapControllerRoute(name:="default", pattern:="{controller=Home}\{action=Index}/{id?}")          ' Noncompliant
        app.MapControllerRoute("default", "{controller=Home}\{action=Index}/{id?}")                         ' Noncompliant
        app.MapAreaControllerRoute("default", "area", "{controller=Home}\{action=Index}/{id?}")             ' Noncompliant
        app.MapFallbackToAreaController("{controller=Home}\{action=Index}", "action", "controller", "area") ' Noncompliant
        app.MapFallbackToAreaController("\action", "\controller", "\area")                                  ' Compliant
        app.MapFallbackToController("\action", "\controller")                                               ' Compliant
        app.MapFallbackToController("{controller=Home}\{action=Index}", "\action", "\controller")           ' Noncompliant

        ControllerEndpointRouteBuilderExtensions.MapControllerRoute(app, "default", "{controller=Home}\{action=Index}/{id?}")  ' Noncompliant
    End Sub
End Class
