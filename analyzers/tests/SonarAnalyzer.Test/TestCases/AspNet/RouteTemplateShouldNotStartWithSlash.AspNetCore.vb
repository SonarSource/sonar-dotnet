Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Mvc.Routing
Imports MyRoute = Microsoft.AspNetCore.Mvc.RouteAttribute
Imports ASP = Microsoft.AspNetCore

<Route("[controller]")>
Public Class NoncompliantController     ' Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
    Inherits Controller
    <Route("/Index1")>                  ' Secondary
    Public Function Index1() As IActionResult
        Return View()
    End Function

    <Route("/SubPath/Index2")>          ' Secondary
    Public Function Index2() As IActionResult
        Return View()
    End Function

    <HttpGet("/[action]")>              ' Secondary
    Public Function Index3() As IActionResult
        Return View()
    End Function

    <HttpGet("/SubPath/Index4_1")>      ' Secondary
    <HttpGet("/[controller]/Index4_2")> ' Secondary
    Public Function Index4() As IActionResult
        Return View()
    End Function
End Class

<Route("[controller]")>
<Route("[controller]/[action]")>
Public Class NoncompliantMultiRouteController ' Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
    Inherits Controller
    <Route("/Index1")>                         ' Secondary
    Public Function Index1() As IActionResult
        Return View()
    End Function

    <Route("/SubPath/Index2")>                  ' Secondary
    Public Function Index2() As IActionResult
        Return View()
    End Function

    <HttpGet("/[action]")>                      ' Secondary
    Public Function Index3() As IActionResult
        Return View()
    End Function

    <HttpGet("/SubPath/Index4_1")>             ' Secondary
    <HttpGet("/[controller]/Index4_2")>        ' Secondary
    Public Function Index4() As IActionResult
        Return View()
    End Function
End Class

<Route("[controller]")>
Public Class CompliantController ' Compliant: at least one action has at least a relative route
    Inherits Controller
    <Route("/Index1")>
    Public Function Index1() As IActionResult
        Return View()
    End Function

    <Route("/SubPath/Index2")>
    Public Function Index2() As IActionResult
        Return View()
    End Function

    <HttpGet("/[action]")>
    Public Function Index3() As IActionResult
        Return View()
    End Function

    <HttpGet("/[controller]/Index4_1")>
    <HttpGet("SubPath/Index4_2")> ' The relative route
    Public Function Index4() As IActionResult
        Return View()
    End Function
End Class

Public Class NoncompliantNoControllerRouteController ' Noncompliant {{Change the paths of the actions of this controller to be relative and add a controller route with the common prefix.}}
    Inherits Controller
    <Route("/Index1")>                               ' Secondary
    Public Function Index1() As IActionResult
        Return View()
    End Function

    <Route("/SubPath/Index2")>                       ' Secondary
    Public Function Index2() As IActionResult
        Return View()
    End Function

    <HttpGet("/[action]")>                           ' Secondary
    Public Function Index3() As IActionResult
        Return View()
    End Function

    <HttpGet("/SubPath/Index4_1")>                  ' Secondary
    <HttpGet("/[controller]/Index4_2")>             ' Secondary
    Public Function Index4() As IActionResult
        Return View()
    End Function
End Class

Public Class CompliantNoControllerRouteNoActionRouteController  ' Compliant
    Inherits Controller
    Public Function Index1() As IActionResult
        Return View()
    End Function ' Default route -> relative

    <Route("/SubPath/Index2")>
    Public Function Index2() As IActionResult
        Return View()
    End Function

    <HttpGet("/[action]")>
    Public Function Index3() As IActionResult
        Return View()
    End Function

    <HttpGet("/SubPath/Index4_1")>
    <HttpGet("/[controller]/Index4_2")>
    Public Function Index4() As IActionResult
        Return View()
    End Function
End Class

Public Class CompliantNoControllerRouteEmptyActionRouteController  ' Compliant
    Inherits Controller
    <HttpGet>
    Public Function Index1() As IActionResult
        Return View()
    End Function ' Empty route -> relative

    <Route("/SubPath/Index2")>
    Public Function Index2() As IActionResult
        Return View()
    End Function

    <HttpGet("/[action]")>
    Public Function Index3() As IActionResult
        Return View()
    End Function

    <HttpGet("/SubPath/Index4_1")>
    <HttpGet("/[controller]/Index4_2")>
    Public Function Index4() As IActionResult
        Return View()
    End Function
End Class

Namespace WithAliases

    Public Class WithAliasedRouteAttributeController  ' Noncompliant
        Inherits Controller
        <MyRoute("/[controller]")>                    ' Secondary
        Public Function Index() As IActionResult
            Return View()
        End Function
    End Class

    Public Class WithFullQualifiedPartiallyAliasedNameController ' Noncompliant
        Inherits Controller
        <ASP.Mvc.RouteAttribute("/[action]")>   ' Secondary
        Public Function Index() As IActionResult
            Return View()
        End Function
    End Class
End Namespace

Public Class MultipleActionsAllRoutesStartingWithSlash1Controller ' Noncompliant
    Inherits Controller
    <HttpGet("/Index1")>                                          ' Secondary
    Public Function WithHttpAttribute() As IActionResult
        Return View()
    End Function

    <Route("/Index2")>                                            ' Secondary
    Public Function WithRouteAttribute() As IActionResult
        Return View()
    End Function
End Class

Public Class MultipleActionsAllRoutesStartingWithSlash2Controller ' Noncompliant
    Inherits Controller
    <HttpGet("/Index1")>                                          ' Secondary
    <HttpGet("/Index3")>                                          ' Secondary
    Public Function WithHttpAttributes() As IActionResult
        Return View()
    End Function

    <Route("/Index2")>                                            ' Secondary
    <Route("/Index4")>                                            ' Secondary
    <HttpGet("/Index5")>                                          ' Secondary
    Public Function WithRouteAndHttpAttributes() As IActionResult
        Return View()
    End Function
End Class

<Route("[controller]")>
Public Class MultipleActionsAllRoutesStartingWithSlash3Controller ' Noncompliant
    Inherits Controller
    <HttpGet("/Index1")>                                          ' Secondary
    <HttpGet("/Index3")>                                          ' Secondary
    Public Function WithHttpAttributes() As IActionResult
        Return View()
    End Function

    <Route("/Index2")>                                            ' Secondary
    <Route("/Index4")>                                            ' Secondary
    <HttpGet("/Index5")>                                          ' Secondary
    Public Function WithRouteAndHttpAttributes() As IActionResult
        Return View()
    End Function
End Class

Public Class MultipleActionsSomeRoutesStartingWithSlash1Controller ' Compliant: some routes are relativ
    Inherits Controller
    <HttpGet("Index1")>
    Public Function WithHttpAttribute() As IActionResult
        Return View()
    End Function

    <Route("/Index2")>
    Public Function WithRouteAttribute() As IActionResult
        Return View()
    End Function
End Class

Public Class MultipleActionsSomeRoutesStartingWithSlash2Controller ' Compliant: some routes are relative
    Inherits Controller
    <HttpGet("Index1")>
    <HttpGet("/Index1")>
    Public Function WithHttpAttributes() As IActionResult
        Return View()
    End Function

    <Route("/Index2")>
    Public Function WithRouteAttribute() As IActionResult
        Return View()
    End Function
End Class

Public Class MultipleActionsSomeRoutesStartingWithSlash3Controller ' Compliant: some routes are relative
    Inherits Controller
    <HttpGet("Index1")>
    <HttpPost("/Index1")>
    Public Function WithHttpAttributes() As IActionResult
        Return View()
    End Function

    <Route("/Index2")>
    Public Function WithRouteAttribute() As IActionResult
        Return View()
    End Function
End Class

<NonController>
Public Class NotAController                     ' Compliant, not a controller
    Inherits Controller
    <Route("/Index1")>
    Public Function Index() As IActionResult
        Return View()
    End Function
End Class

Public Class ControllerWithoutControllerSuffix  ' Noncompliant
    Inherits Controller
    <Route("/Index1")>                          ' Secondary
    Public Function Index() As IActionResult
        Return View()
    End Function
End Class

<Controller>
Public Class ControllerWithControllerAttribute  ' Noncompliant
    Inherits Controller
    <Route("/Index1")>                          ' Secondary
    Public Function Index() As IActionResult
        Return View()
    End Function
End Class

Public Class ControllerWithoutParameterlessConstructor  ' Noncompliant
    Inherits Controller
    Public Sub New(i As Integer)
    End Sub

    <Route("/Index1")>                                  ' Secondary
    Public Function Index() As IActionResult
        Return View()
    End Function
End Class

Public Class ControllerRequirementsInfluenceActionsCheck

    Friend Class InternalController ' Compliant, actions in nested classes are not reachable
        Inherits Controller
        <Route("/Index1")>
        Public Function Index() As IActionResult
            Return View()
        End Function
    End Class

    Protected Class ProtectedController ' Compliant, actions in nested classes are not reachable
        Inherits Controller
        <Route("/Index1")>
        Public Function Index() As IActionResult
            Return View()
        End Function
    End Class

End Class

' https://github.com/SonarSource/sonar-dotnet/issues/9002
Public Class Repro_9002                     ' Noncompliant
    Inherits Controller
    
    <Route("~/B")>                          ' Secondary
    Public Function Index() As ActionResult
        Return View()
    End Function
End Class
