Imports Microsoft.AspNetCore.Builder

Class WithAllControllerEndpointRouteBuilderExtensionsMethods
    ' MapFallbackToPage decorates the pattern with StringSyntax("Route") since ASP.NET Core 8.0.0
    Private Sub MapControllerRoute(ByVal app As WebApplication)
        app.MapFallbackToPage("\action")                                   ' Compliant
        app.MapFallbackToPage("{controller=Home}\{action=Index}", "\page") ' Noncompliant
        '                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    End Sub
End Class
