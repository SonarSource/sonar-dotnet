Imports System
Imports System.Web.Mvc

<Route("A\[controller]")>   ' Noncompliant ^8#16 {{Replace '\' with '/'.}}
Public Class BackslashOnController
    Inherits Controller
End Class

<Route("A\[controller]\B")> ' Noncompliant ^8#18 {{Replace '\' with '/'.}}
Public Class MultipleBackslashesOnController
    Inherits Controller
End Class

Public Class BackslashOnActionController
    Inherits Controller

    <Route("A\[action]")>   ' Noncompliant ^12#12 {{Replace '\' with '/'.}}
    Public Function Index() As ActionResult
        Return View()
    End Function
End Class

Public Class MultipleBackslashesOnActionController
    Inherits Controller

    <Route("A\[action]\B")> ' Noncompliant ^12#14 {{Replace '\' with '/'.}}
    Public Function Index() As ActionResult
        Return View()
    End Function
End Class

<Route("\[controller]")>    ' Noncompliant
Public Class RouteOnControllerStartingWithBackslashController
    Inherits Controller
End Class

Public Class AController
    Inherits Controller

    Public Function WithoutRouteAttribute() As ActionResult             ' Compliant
        Return View()
    End Function

    <Route("A\[action]", Name:="a", Order:=3)>                          ' Noncompliant
    Public Function WithOptionalAttributeParameters() As ActionResult
        Return View()
    End Function

    <RouteAttribute("A\[action]")>                                      ' Noncompliant
    Public Function WithAttributeSuffix() As ActionResult
        Return View()
    End Function

    <System.Web.Mvc.RouteAttribute("A\[action]")>                       ' Noncompliant
    Public Function WithFullQualifiedName() As ActionResult
        Return View()
    End Function

    <Route("A\[action]")>                                               ' Noncompliant
    <Route("B\[action]")>                                               ' Noncompliant
    <Route("C/[action]")>                                               ' Compliant: forward slash is used
    Public Function WithMultipleRoutes() As ActionResult
        Return View()
    End Function

    <Route("A%5C[action]")>                                             ' Compliant: URL-escaped backslash is used
    Public Function WithUrlEscapedBackslash() As ActionResult
        Return View()
    End Function

    <Route("A/{s:regex(^(?!index\b)[[a-zA-Z0-9-]]+$)}.html")>           ' Compliant: backslash is in regex
    Public Function WithRegexContainingBackslashInLookahead() As ActionResult
        Return View()
    End Function

    <Route("A/{s:datetime:regex(\d{{4}}-\d{{2}}-\d{{4}})}/B")>          ' Compliant: backslash is in regex
    Public Function WithRegexContainingBackslashInMetaEscape() As ActionResult
        Return View()
    End Function
End Class

' https://github.com/SonarSource/sonar-dotnet/issues/9193
Namespace AttributeWithNamedArgument
    <AttributeUsage(AttributeTargets.All)>
    Public Class MyAttribute
        Inherits Attribute
        Public Property Name As String
    End Class

    Public Class MyController
        Inherits Controller
        <MyAttribute(Name:="Display HR\Recruitment report")>
        Public Const Text As String = "ABC"
    End Class
End Namespace
