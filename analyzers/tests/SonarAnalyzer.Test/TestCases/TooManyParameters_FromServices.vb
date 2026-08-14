Imports System
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.DependencyInjection

' https://sonarsource.atlassian.net/browse/NET-4245

Namespace Other

    <AttributeUsage(AttributeTargets.Parameter)>
    Public NotInheritable Class FromServicesAttribute
        Inherits Attribute
    End Class

End Namespace

Namespace Tests.Diagnostics

    Public Class Methods

        Public Sub ExcludesServices(<FromServices> service As Object, p1 As Integer, p2 As Integer, p3 As Integer)      ' Compliant, the injected parameter is not counted
        End Sub

        Public Sub ExcludesKeyedServices(<FromKeyedServices("key")> service As Object, p1 As Integer, p2 As Integer, p3 As Integer)     ' Compliant
        End Sub

        Public Sub CountsTheRemainingParameters(<FromServices> service As Object, p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer)   ' Noncompliant {{Sub has 4 parameters, which is greater than the 3 authorized.}}
        End Sub

        Public Sub ExcludesEveryInjectedParameter(<FromServices> first As Object, <FromKeyedServices("key")> second As Object, p1 As Integer, p2 As Integer, p3 As Integer)   ' Compliant
        End Sub

        Public Sub CountsSameNamedAttributeFromAnotherNamespace(<Global.Other.FromServices> value As Object, p1 As Integer, p2 As Integer, p3 As Integer)    ' Noncompliant {{Sub has 4 parameters, which is greater than the 3 authorized.}}
        End Sub

        Public Function CountsUnannotatedParameters(service As Object, p1 As Integer, p2 As Integer, p3 As Integer) As Integer         ' Noncompliant {{Function has 4 parameters, which is greater than the 3 authorized.}}
            Return 0
        End Function

    End Class

    Public Class Base

        Public Sub New(service As Object)
        End Sub

    End Class

    Public Class Derived
        Inherits Base

        ' The injected parameter is already excluded from the parameter count, so it must not be subtracted a second time as a base constructor argument
        Public Sub New(<FromServices> service As Object, p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer)    ' Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}
            MyBase.New(service)
        End Sub

    End Class

    Public Class DerivedRegular
        Inherits Base

        Public Sub New(service As Object, p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer)                   ' Noncompliant {{Constructor has 4 new parameters, which is greater than the 3 authorized.}}
            MyBase.New(service)
        End Sub

    End Class

    Public Class DerivedMixed
        Inherits Base

        Public Sub New(<FromServices> service As Object, forwarded As Object, p1 As Integer, p2 As Integer, p3 As Integer)  ' Compliant, only 'forwarded' is subtracted as a base constructor argument
            MyBase.New(forwarded)
        End Sub

    End Class

    Public Class PlainBase

        Public Sub New()
        End Sub

    End Class

    Public Class NoParentheses
        Inherits PlainBase

        ' A call without parentheses has no argument list
        Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer)                                          ' Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}
            MyBase.New
        End Sub

    End Class

    Public Class OptionalBase

        Public Sub New(Optional first As Integer = 1, Optional second As Integer = 2)
        End Sub

    End Class

    Public Class OmittedArgument
        Inherits OptionalBase

        ' An omitted argument has no expression to resolve
        Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer, p5 As Integer, p6 As Integer)        ' Noncompliant {{Constructor has 4 new parameters, which is greater than the 3 authorized.}}
            MyBase.New(, 2)
        End Sub

    End Class

End Namespace
