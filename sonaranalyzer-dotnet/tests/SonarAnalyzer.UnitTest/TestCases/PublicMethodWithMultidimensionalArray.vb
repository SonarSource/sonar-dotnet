Imports System

Namespace Tests.Diagnostics
    Public Interface IFace
        Sub Method6(a As Integer()()) ' Noncompliant {{Make this method private or simplify its parameters to not use multidimensional arrays.}}
'           ^^^^^^^
    End Interface

    Public Class Base
        MustOverride Sub Method5(a As Integer()()) ' Noncompliant
    End Class

    Public Class PublicMethodWithMultidimensionalArray
        Inherits Base
        Implements IFace
        Public Sub Method(a As Integer)
        End Sub
        Public Sub MethodX(a As Integer())
        End Sub

        Public Sub Method1(a As Integer()()) ' Noncompliant
        End Sub
        Sub Method2(a As Integer()()) ' Noncompliant
        End Sub
        Sub Method2b(a As Integer(,)) ' Noncompliant
        End Sub

        Private Sub Method3(a As Integer()())
        End Sub

        Function Method4(a As Integer()()) As Integer ' Noncompliant
            Return 1
        End Function

        Overrides Sub Method5(a As Integer()()) ' Compliant
        End Sub

        Private Sub Method6(a As Integer()()) Implements IFace.Method6 ' Compliant, interface implementation
            Throw New NotImplementedException()
        End Sub
    End Class
    Friend Class Other

        Public Sub Method1(a As Integer()()) ' Compliant
        End Sub

    End Class
End Namespace