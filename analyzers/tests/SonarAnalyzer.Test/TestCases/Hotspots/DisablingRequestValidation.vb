Imports System
Imports System.Web.Mvc

Namespace Tests.Diagnostics
    <ValidateInput(False)> ' Noncompliant {{Make sure disabling ASP.NET Request Validation feature is safe here.}}
    Public Class NonCompliantClass
    End Class

    Public Class NonCompliantMethods ' we don't care if it derives from controller

        <ValidateInput(False)> Public Function Foo(ByVal input As String) As ActionResult ' Noncompliant {{Make sure disabling ASP.NET Request Validation feature is safe here.}}
'        ^^^^^^^^^^^^^^^^^^^^
            Return Nothing
        End Function

        <CLSCompliant(False)>
        <ValidateInput(False)> ' Noncompliant
        Public Function WithTwoFalse(ByVal input As String) As ActionResult
            Return Foo(input)
        End Function

        <HttpPost>
        <ValidateInput(False)> ' Noncompliant
        <Obsolete>
        Public Function FooWithMoreAttributes(ByVal input As String) As ActionResult
            Return Foo(input)
        End Function

        <ValidateInput(False)> ' Noncompliant
        Public Function FooNoParam() As ActionResult
            Return Nothing
        End Function

        <ValidateInput(False)> ' Noncompliant
        Public Sub VoidFoo()
        End Sub

        <ValidateInput(False)> ' Noncompliant
        Private Function ArrowFoo(ByVal i As String) As ActionResult
            Return Nothing
        End Function
    End Class

    Public Class CompliantController
        Inherits Controller

        <ValidateInput(True)>
        Public Function Foo(ByVal input As String) As ActionResult
            Return Foo(input)
        End Function

        Public Function Bar() As ActionResult
            Return Nothing
        End Function

        <HttpPost>
        Public Function Boo(ByVal input As AllowedHtml) As ActionResult
            Return Nothing
        End Function

        <System.Web.Mvc.HttpPost>
        <System.Web.Mvc.ValidateInput(True)>
        Public Function Qix(ByVal input As String) As ActionResult
            Return Foo(input)
        End Function

        Private Function Quix(ByVal i As String) As ActionResult
            Return Nothing
        End Function
    End Class

    <ValidateInput(True)>
    Public Class CompliantController2
    End Class

    Public Class AllowedHtml
        <AllowHtml>
        Public Property Prop As String
    End Class

    <Obsolete>
    Public Class MyObsoleteClass ' for coverage
    End Class

    <Obsolete("", False)>
    Public Class MyObsoleteClass2 ' for coverage
    End Class

    <CLSCompliant(False)>
    Public Class ClassWithFalse ' for coverage
    End Class

    Public Class Errors
        <ValidateInput("foo")> ' Error [BC30934]
        Public Function Foo(ByVal input As String) As ActionResult
            Return Foo(input)
        End Function

        <ValidateInput()> ' Error [BC30455]
        Public Function Bar(ByVal input As String) As ActionResult
            Return Foo(input)
        End Function

        <ValidateInput(False, "foo")> ' Error [BC30057]
        Public Function Baz(ByVal input As String) As ActionResult
            Return Foo(input)
        End Function
    End Class
End Namespace
