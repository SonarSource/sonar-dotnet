Namespace Samples

    Public Class Address
        Public Function GetZipCode() As String
            Return "12345"
        End Function
    End Class

    Public Structure Person
        Public Function GetFullName() As String
            Return "John Doe"
        End Function
    End Structure

    Public Enum EnumDeclaration
        Value1
        Value2
    End Enum

    Public Class Visibility
        Public Sub PublicMethod()
        End Sub

        Protected Friend Sub ProtectedFriendMethod()
        End Sub

        Protected Sub ProtectedMethod()
        End Sub

        Friend Sub FriendMethod()
        End Sub

        Private Protected Sub PrivateProtectedMethod()
        End Sub

        Private Sub PrivateMethod()
        End Sub

        Sub NoAccessModifierMethod()
        End Sub

        Friend Class FriendClass
            Public Sub Method()
            End Sub
        End Class

        Private Class PrivateClass
            Public Sub Method()
            End Sub
        End Class
    End Class

    Class NoModifiers
        Public Sub Method()
        End Sub
    End Class

    Public Class MultipleMethods
        Public Sub Method1()
        End Sub

        Public Sub Method2()
        End Sub
    End Class

    Public Class OverloadedMethods
        Public Sub Method()
        End Sub

        Public Sub Method(i As Integer)
        End Sub

        Public Sub Method(s As String)
        End Sub

        Public Sub Method(i As Integer, s As String)
        End Sub
    End Class

    Public Class GenericClass(Of T)
        Public Sub Method()
        End Sub

        Public Sub Method(Of U)()
        End Sub
    End Class

    Public Class WithGenericMethod
        Public Sub Method(Of T)()
        End Sub
    End Class

    Partial Public Class PartialClass
        Public Sub InFirstFile()
        End Sub

        Private Partial Sub PartialMethod()
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

End Namespace
