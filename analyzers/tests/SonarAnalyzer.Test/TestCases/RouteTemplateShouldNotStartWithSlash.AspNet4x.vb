Imports System.Web.Mvc

<Route("[controller]")>
Public Class NoncompliantController     ' Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
    Inherits Controller
    <Route("/Index1")>                  ' Secondary
    Public Function Index1() As ActionResult
        Return View()
    End Function

    <Route("/SubPath/Index2_1")>        ' Secondary
    <Route("/[controller]/Index2_2")>   ' Secondary
    Public Function Index2() As ActionResult
        Return View()
    End Function

    <Route("/[action]")>                ' Secondary
    Public Function Index3() As ActionResult
        Return View()
    End Function

    <Route("/SubPath/Index4_1")>        ' Secondary
    <Route("/[controller]/Index4_2")>   ' Secondary
    Public Function Index4() As ActionResult
        Return View()
    End Function
End Class

<RoutePrefix("[controller]")>
Public Class NoncompliantWithRoutePrefixController ' Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
    Inherits Controller
    <Route("/Index1")>                             ' Secondary
    Public Function Index1() As ActionResult
        Return View()
    End Function
End Class

<Route("[controller]")>
<Route("[controller]/[action]")>
Public Class NoncompliantMultiRouteController ' Noncompliant {{Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.}}
    Inherits Controller
    <Route("/Index1")>                        ' Secondary
    Public Function Index1() As ActionResult
        Return View()
    End Function

    <Route("/SubPath/Index2_1")>              ' Secondary
    <Route("/[controller]/Index2_2")>         ' Secondary
    Public Function Index2() As ActionResult
        Return View()
    End Function

    <Route("/[action]")>                      ' Secondary
    Public Function Index3() As ActionResult
        Return View()
    End Function
End Class

<Route("[controller]")>
Public Class CompliantController ' Compliant: at least one action has at least a relative route
    Inherits Controller
    <Route("/Index1")>
    Public Function Index1() As ActionResult
        Return View()
    End Function

    <Route("/SubPath/Index2")>
    Public Function Index2() As ActionResult
        Return View()
    End Function

    <Route("/[action]")>
    Public Function Index3() As ActionResult
        Return View()
    End Function

    <Route("/[controller]/Index4_1")>
    <Route("SubPath/Index4_2")> ' The relative route
    Public Function Index4() As ActionResult
        Return View()
    End Function
End Class

Public Class NoncompliantNoControllerRouteController ' Noncompliant {{Change the paths of the actions of this controller to be relative and add a controller route with the common prefix.}}
    Inherits Controller
    <Route("/Index1")>                               ' Secondary
    Public Function Index1() As ActionResult
        Return View()
    End Function

    <Route("/SubPath/Index2_1")>                     ' Secondary
    <Route("/[controller]/Index2_2")>                ' Secondary
    Public Function Index2() As ActionResult
        Return View()
    End Function

    <Route("/[action]")>                             ' Secondary
    Public Function Index3() As ActionResult
        Return View()
    End Function
End Class

Public Class CompliantNoControllerRouteNoActionRouteController ' Compliant
    Inherits Controller
    Public Function Index1() As ActionResult
        Return View()
    End Function ' Default route -> relative

    <Route("/SubPath/Index2")>
    Public Function Index2() As ActionResult
        Return View()
    End Function

    <Route("/[action]")>
    Public Function Index3() As ActionResult
        Return View()
    End Function

    <Route("/SubPath/Index4_1")>
    <Route("/[controller]/Index4_2")>
    Public Function Index4() As ActionResult
        Return View()
    End Function
End Class

Public Class CompliantNoControllerRouteEmptyActionRouteController ' Compliant
    Inherits Controller
    <Route>
    Public Function Index1() As ActionResult
        Return View()
    End Function ' Empty route -> relative

    <Route("/SubPath/Index2")>
    Public Function Index2() As ActionResult
        Return View()
    End Function

    <Route("/[action]")>
    Public Function Index3() As ActionResult
        Return View()
    End Function

    <Route("/SubPath/Index4_1")>
    <Route("/[controller]/Index4_2")>
    Public Function Index4() As ActionResult
        Return View()
    End Function
End Class
