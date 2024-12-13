Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace Samples.VB

    Public Class Address
        <TestMethod>
        Public Function GetZipCode() As String
            Return "12345"
        End Function
    End Class

    Public Structure Person
        <TestMethod>
        Public Function GetFullName() As String
            Return "John Doe"
        End Function
    End Structure

    Public Enum EnumDeclaration
        Value1
        Value2
    End Enum

    Public Class Visibility
        <TestMethod>
        Public Sub PublicMethod()
        End Sub

        <TestMethod>
        Protected Friend Sub ProtectedFriendMethod()
        End Sub

        <TestMethod>
        Protected Sub ProtectedMethod()
        End Sub

        <TestMethod>
        Friend Sub FriendMethod()
        End Sub

        <TestMethod>
        Private Protected Sub PrivateProtectedMethod()
        End Sub

        <TestMethod>
        Private Sub PrivateMethod()
        End Sub

        <TestMethod>
        Sub NoAccessModifierMethod()
        End Sub

        Friend Class FriendClass
            <TestMethod>
            Public Sub Method()
            End Sub
        End Class

        Private Class PrivateClass
            <TestMethod>
            Public Sub Method()
            End Sub
        End Class
    End Class

    Class NoModifiers
        <TestMethod>
        Public Sub Method()
        End Sub
    End Class

    Public Class MultipleMethods
        <TestMethod>
        Public Sub Method1()
        End Sub

        <TestMethod>
        Public Sub Method2()
        End Sub
    End Class

    Public Class OverloadedMethods
        <TestMethod>
        Public Sub Method()
        End Sub

        <TestMethod>
        Public Sub Method(i As Integer)
        End Sub

        <TestMethod>
        Public Sub Method(s As String)
        End Sub

        <TestMethod>
        Public Sub Method(i As Integer, s As String)
        End Sub
    End Class

    Public Class GenericClass(Of T)
        <TestMethod>
        Public Sub Method()
        End Sub

        <TestMethod>
        Public Sub Method(Of U)()
        End Sub
    End Class

    Public Class WithGenericMethod
        <TestMethod>
        Public Sub Method(Of T)()
        End Sub
    End Class

    Partial Public Class PartialClass
        Inherits BaseClass

        <TestMethod>
        Public Sub InFirstFile()
        End Sub

        Private Partial Sub PartialMethod() ' Test method attribute is defined in the other file.
        End Sub
    End Class

    Public Class PropertiesAndIndexers
        Private values() As Integer

        Public Property [Property] As String

        Default Public Property Item(index As Integer) As Integer
            Get
                Return values(index)
            End Get
            Set(value As Integer)
                values(index) = value
            End Set
        End Property
    End Class

    Public MustInherit Class BaseClass
        <TestMethod>
        Public Sub BaseClassMethod()
        End Sub

        <TestMethod>
        Public Overridable Sub Method()
        End Sub
    End Class

    Public Class DerivedClass
        Inherits BaseClass

        <TestMethod>
        Public Overrides Sub Method()
        End Sub
    End Class

    Public Class MultipleLevelInheritance
        Inherits DerivedClass

        <TestMethod>
        Public Sub MultipleLevelInheritanceMethod()
        End Sub
    End Class

    Public Interface IInterfaceWithTestDeclarations
        <TestMethod>
        Function GetZipCode() As String
    End Interface

End Namespace
