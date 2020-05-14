Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks

Namespace Tests.Diagnostics
    Public Class Class0
        Public Sub M()
        End Sub
    End Class

    Public Class Class1 ' Noncompliant {{This class can't be instantiated; make its constructor 'public'.}}
'                ^^^^^^
        Private Sub New()
        End Sub
    End Class

    Public NotInheritable Class Class1b ' Noncompliant {{This class can't be instantiated; make at least one of its constructors 'public'.}}
        Private Sub New()
        End Sub

        Private Sub New(ByVal i As Integer)
        End Sub

        Public Sub M()
        End Sub
    End Class

    Public Class Class2 ' Compliant, suggested solution of S1118
        Private Sub New()
        End Sub

        Public Shared Sub M()
        End Sub
    End Class

    Public NotInheritable Class Class3 ' Compliant
        Private Sub New()
        End Sub

        Public Sub M()
        End Sub

        Public Shared ReadOnly Property instance As Class3
            Get
                Return New Class3()
            End Get
        End Property
    End Class

    Public NotInheritable Class Class4 ' Compliant
        Public Sub M()
        End Sub
    End Class

    Public Class Class6 ' Compliant
        Private Sub New()
        End Sub

        Public Class Intermediate
            Public Class Nested ' Noncompliant
                Inherits Class6

                Private Sub New()
                End Sub
            End Class
        End Class
    End Class

    Public Class MyClassGeneric(Of T)
        Private Sub New()
        End Sub

        Public Class Nested
            Inherits MyClassGeneric(Of Integer)
        End Class
    End Class

    Public Class MyClassGeneric2(Of T)
        Private Sub New()
        End Sub

        Public Function Create() As Object
            Return New MyClassGeneric2(Of Integer)()
        End Function
    End Class

    Public Class MyAttribute
        Inherits System.Attribute
    End Class

    <My>
    Public Class WithAttribute1
        Private Sub New()
        End Sub
    End Class

    Public Class WithAttribute2
        <My>
        Private Sub New()
        End Sub
    End Class

    ' https//github.com/SonarSource/sonar-dotnet/issues/3329
    Public Class Repro_3329 ' Compliant, instance will be created by PInvoke of DllImport
        Inherits System.Runtime.InteropServices.SafeHandle

        Private Sub New()
            MyBase.New(System.IntPtr.Zero, True)
        End Sub

        Public Overrides ReadOnly Property IsInvalid As Boolean
            Get
            End Get
        End Property

        Protected Overrides Function ReleaseHandle() As Boolean
        End Function

    End Class

End Namespace
