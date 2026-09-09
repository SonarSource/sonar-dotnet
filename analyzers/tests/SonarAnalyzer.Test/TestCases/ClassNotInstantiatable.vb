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

    Public Class ClassWithOnlyConstants ' Compliant, suggested solution of S1118 - Constants are Shared, even when they are Private
        Private Sub New()
        End Sub

        Private Const Value As Integer = 1
    End Class

    ' https://sonarsource.atlassian.net/browse/NET-4315
    Public Class ClassWithPrivateNestedType ' Compliant - Only Shared members and nested types, none of which needs an instance of the containing type
        Private Sub New()
        End Sub

        Public Shared Function Compute() As Integer
            Return New Nested().GetValue()
        End Function

        Private NotInheritable Class Nested
            Public Function GetValue() As Integer
                Return GetHashCode()
            End Function
        End Class
    End Class

    ' https://sonarsource.atlassian.net/browse/NET-4315
    Public Class ClassWithOnlyNestedType ' Compliant - Nested types are accessible without an instance of the containing type
        Private Sub New()
        End Sub

        Public Class Nested
        End Class
    End Class

    ' The exemption applies to every nested type kind, not just nested classes
    Public Class ClassWithOnlyNestedStructure ' Compliant
        Private Sub New()
        End Sub

        Public Structure Nested
            Public Field As Integer
        End Structure
    End Class

    Public Class ClassWithOnlyNestedEnum ' Compliant
        Private Sub New()
        End Sub

        Public Enum Nested
            A
        End Enum
    End Class

    Public Class ClassWithOnlyNestedInterface ' Compliant
        Private Sub New()
        End Sub

        Public Interface INested
        End Interface
    End Class

    Public Class ClassWithOnlyNestedDelegate ' Compliant
        Private Sub New()
        End Sub

        Public Delegate Sub NestedDelegate()
    End Class

    Public Class ClassWithOnlyFriendNestedType ' Compliant - A Friend nested type is reachable from the rest of the assembly
        Private Sub New()
        End Sub

        Friend Class Nested
        End Class
    End Class

    Public Class ClassWithOnlyProtectedFriendNestedType ' Compliant - 'Protected Friend' is also Friend, so it is reachable from the rest of the assembly
        Private Sub New()
        End Sub

        Protected Friend Class Nested
        End Class
    End Class

    Public Class ClassWithOnlyProtectedNestedType ' Noncompliant - All the constructors are private, so nothing outside can derive from the class and reach the nested type
        Private Sub New()
        End Sub

        Protected Class Nested
        End Class
    End Class

    Public Class ClassWithPrivateAndPublicNestedTypes ' Compliant - It is enough that one of the nested types is reachable from the outside
        Private Sub New()
        End Sub

        Private Structure Hidden
            Public Field As Integer
        End Structure

        Public Enum Visible
            A
        End Enum
    End Class

    Public Class ClassWithOnlyPrivateNestedType ' Noncompliant - Neither the class nor its private nested type can be reached from the outside
        Private Sub New()
        End Sub

        Private Class Nested
        End Class
    End Class

    Public Class ClassWithOnlyPrivateNestedTypes ' Noncompliant - Same as above, no matter the kind of the nested types
        Private Sub New()
        End Sub

        Private Structure Hidden
            Public Field As Integer
        End Structure

        Private Enum AlsoHidden
            A
        End Enum
    End Class

    Public Structure StructureWithPrivateConstructor ' Compliant - Only classes are checked, a structure is always instantiatable
        Private Sub New(ByVal i As Integer)
        End Sub
    End Structure

    Public Class ClassWithPrivateNestedType2 ' Noncompliant
        Private Sub New()
        End Sub

        Public Shared Function Compute() As Integer
            Return New Nested().GetValue()
        End Function

        Public Function InstanceCompute() As Integer
            Return New Nested().GetValue()
        End Function

        Private NotInheritable Class Nested
            Public Function GetValue() As Integer
                Return GetHashCode()
            End Function
        End Class
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

        ' Instance member, so the class is not exempted and the nested type inheriting from it is what makes it compliant
        Public Sub M()
        End Sub

        Public Class Intermediate
            Public Class Nested ' Noncompliant
                Inherits Class6

                Private Sub New()
                End Sub
            End Class
        End Class
    End Class

    Public Class MyClassGeneric(Of T) ' Compliant
        Private Sub New()
        End Sub

        ' Instance member, so the class is not exempted and the nested type inheriting from it is what makes it compliant
        Public Sub M()
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
