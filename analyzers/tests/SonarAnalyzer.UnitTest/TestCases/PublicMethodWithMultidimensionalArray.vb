Imports System

Namespace Tests.Diagnostics
    Public Interface IFace
        Sub Method6(a As Integer()()) ' Noncompliant {{Make this method private or simplify its parameters to not use multidimensional/jagged arrays.}}
'           ^^^^^^^
    End Interface

    Public MustInherit Class Base
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

        Public Sub Method7(ParamArray a As Integer()()) ' Compliant
        End Sub

        Public Sub Method8(ParamArray a As Integer()()()) ' Noncompliant
        End Sub

        Sub Method9(ParamArray a As Integer()(,)) ' Compliant
        End Sub

        Sub Method10 ' Compliant
            Console.WriteLine("Hello, world!")
        End Sub

        Public Sub Metho11(a As Integer()(), ParamArray b As Integer()()) ' Noncompliant
        End Sub

        Sub Method12(a As Integer(,)()) ' Noncompliant
        End Sub

    End Class
    Friend Class Other

        Public Sub Method1(a As Integer()()) ' Compliant
        End Sub

    End Class

    Public Class Constructors
        Public Sub New(A As Integer()()) ' Noncompliant {{Make this constructor private or simplify its parameters to not use multidimensional/jagged arrays.}}
'                  ^^^
        End Sub

        Public Sub New(A As Integer(,)) ' Noncompliant
        End Sub

        Public Sub New(ParamArray A As Integer())
        End Sub

        Public Sub New(ParamArray A As Integer()()()) ' Noncompliant
        End Sub

        Public Sub New(I As Integer)
        End Sub
    End Class

End Namespace
