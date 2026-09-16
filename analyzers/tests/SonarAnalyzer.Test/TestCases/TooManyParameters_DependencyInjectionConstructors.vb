Imports System
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.DependencyInjection

Public Class ActivationConstructor

    Public Sub New()
    End Sub

    <ActivatorUtilitiesConstructor>
    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
    End Sub

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer, p5 As Integer) ' Noncompliant
    End Sub

    Public Sub Method(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
    End Sub

End Class

Public Class DerivedFromActivationConstructor
    Inherits ActivationConstructor

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant, constructor attributes are not inherited
    End Sub

End Class

Public Structure ActivationStruct

    <ActivatorUtilitiesConstructor>
    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
    End Sub

End Structure

Public Class ActualController
    Inherits Microsoft.AspNetCore.Mvc.ControllerBase

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
    End Sub

    Public Sub Action(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
    End Sub

End Class

<ApiController>
Public Class AnnotatedApiController

    Public Sub New()
    End Sub

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
    End Sub

    Public Sub Action(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
    End Sub

End Class

<Controller>
Public Class AnnotatedController

    Public Sub New()
    End Sub

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
    End Sub

    Public Sub Action(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
    End Sub

End Class

Public Class InheritedApiController
    Inherits AnnotatedApiController

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
    End Sub

End Class

Public Class InheritedController
    Inherits AnnotatedController

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
    End Sub

End Class

<CustomController>
Public Class CustomAnnotatedController

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
    End Sub

End Class

Public Class CustomControllerAttribute
    Inherits ControllerAttribute
End Class

<AttributeUsage(AttributeTargets.Class, Inherited:=False)>
Public Class NonInheritedControllerAttribute
    Inherits ControllerAttribute
End Class

<NonInheritedController>
Public Class NonInheritedControllerBase
End Class

Public Class DerivedFromNonInheritedController
    Inherits NonInheritedControllerBase

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
    End Sub

End Class

<NonController>
Public Class OptedOutController
    Inherits Microsoft.AspNetCore.Mvc.ControllerBase

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant, <NonController> opts the type out of MVC activation
    End Sub

End Class

Public Class FilterAttribute    ' ActionFilterAttribute implements IFilterMetadata through IActionFilter
    Inherits Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant, the arguments are written at every <Filter(...)> usage site
    End Sub

End Class

Public Class ActivatedFilterAttribute
    Inherits Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute

    <ActivatorUtilitiesConstructor>
    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant, explicitly marked as the activation constructor
    End Sub

End Class

Public Class ServiceFilter    ' A filter that is not an attribute is resolved from the container
    Implements Microsoft.AspNetCore.Mvc.Filters.IActionFilter

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
    End Sub

    Public Sub OnActionExecuting(context As Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext) Implements Microsoft.AspNetCore.Mvc.Filters.IActionFilter.OnActionExecuting
    End Sub

    Public Sub OnActionExecuted(context As Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext) Implements Microsoft.AspNetCore.Mvc.Filters.IActionFilter.OnActionExecuted
    End Sub

End Class

Public Class OrdinaryService

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
    End Sub

End Class

<Other.ApiController, Other.Controller>
Public Class UnrelatedAttributes

    <Other.ActivatorUtilitiesConstructor>
    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
    End Sub

End Class

Public Class UnrelatedBaseType
    Inherits Other.Controller

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
    End Sub

End Class

Public Class UnrelatedInterface
    Implements Other.IHostedService

    Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
    End Sub

End Class

Namespace Other

    Public Class ApiControllerAttribute
        Inherits Attribute
    End Class

    Public Class ControllerAttribute
        Inherits Attribute
    End Class

    Public Class ActivatorUtilitiesConstructorAttribute
        Inherits Attribute
    End Class

    Public Class Controller
    End Class

    Public Interface IHostedService
    End Interface

End Namespace
