Imports Microsoft.AspNetCore.Mvc

<Route("A\[controller]")>   ' Noncompliant ^8#16 {{Replace '\' with '/'.}}
Public Class BackslashOnController : Inherits Controller : End Class

<Route("A\[controller]\B")> ' Noncompliant ^8#18 {{Replace '\' with '/'.}}
Public Class MultipleBackslashesOnController : Inherits Controller : End Class

Public Class BackslashOnActionController
    Inherits Controller

    <Route("A\[action]")>   ' Noncompliant ^12#12 {{Replace '\' with '/'.}}
    Public Function Index() As IActionResult
        Return View()
    End Function
End Class

Public Class MultipleBackslashesOnActionController
    Inherits Controller

    <Route("A\[action]\B")> ' Noncompliant ^12#14 {{Replace '\' with '/'.}}
    Public Function Index() As IActionResult
        Return View()
    End Function
End Class

<Route("\[controller]")>    ' Noncompliant
Public Class RouteOnControllerStartingWithBackslashController
    Inherits Controller
End Class

Public Class AController
    Inherits Controller

    ' [Route("A\\[action]")] ' Compliant: commented out
    Public Function WithoutRouteAttribute() As IActionResult
        Return View()
    End Function

    <Route("A\[action]", Name:="a", Order:=3)>   ' Noncompliant
    Public Function WithOptionalAttributeParameters() As IActionResult
        Return View()
    End Function

    <Route("A/[action]", Name:="a\b", Order:=3)> ' Compliant: backslash is on the name
    Public Function WithBackslashInRouteName() As IActionResult
        Return View()
    End Function

    <RouteAttribute("A\[action]")>               ' Noncompliant
    Public Function WithAttributeSuffix() As IActionResult
        Return View()
    End Function

    <Microsoft.AspNetCore.Mvc.RouteAttribute("A\[action]")> ' Noncompliant
    Public Function WithFullQualifiedName() As IActionResult
        Return View()
    End Function

    <Route("A\[action]")>   ' Noncompliant
    <Route("B\[action]")>   ' Noncompliant
    <Route("C/[action]")>   ' Compliant: forward slash is used
    Public Function WithMultipleRoutes() As IActionResult
        Return View()
    End Function

    <Route("A%5C[action]")> ' Compliant: URL-escaped backslash is used
    Public Function WithUrlEscapedBackslash() As IActionResult
        Return View()
    End Function

    <Route("A/{s:regex(^(?!index\b)[[a-zA-Z0-9-]]+$)}.html")>  ' Compliant: backslash is in regex
    Public Function WithRegexContainingBackslashInLookahead(ByVal s As String) As IActionResult
        Return View()
    End Function

    <Route("A/{s:datetime:regex(\d{{4}}-\d{{2}}-\d{{4}})}/B")> ' Compliant: backslash is in regex
    Public Function WithRegexContainingBackslashInMetaEscape(ByVal s As String) As IActionResult
        Return View()
    End Function
End Class

Public Class WithAllTypesOfStringsController
    Inherits Controller

    Private Const AConstStringIncludingABackslash As String = "A\"
    Private Const AConstStringNotIncludingABackslash As String = "A/"

    <Route(AConstStringIncludingABackslash)>    ' Noncompliant
    Public Function WithConstStringIncludingABackslash() As IActionResult
        Return View()
    End Function

    <Route(AConstStringNotIncludingABackslash)> ' Compliant
    Public Function WithConstStringNotIncludingABackslash() As IActionResult
        Return View()
    End Function
End Class

