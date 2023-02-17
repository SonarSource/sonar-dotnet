Imports System

Class Empty                                         ' Noncompliant {{Remove this empty class, or add members to it.}}
    ' ^^^^^
End Class


Public Class PublicEmpty                            ' Noncompliant
End Class

Class InternalEmpty                                 ' Noncompliant
End Class

Class EmptyWithComments                             ' Noncompliant
    ' Some comment
End Class

Class ClassWithProperty
    Private ReadOnly Property SomeProperty As Integer
        Get
            Return 42
        End Get
    End Property
End Class

Class ClassWithField
    Private SomeField As Integer = 42
End Class

Class ClassWithMethod
    Private Sub Method()
    End Sub
End Class

Class ClassWithIndexer
    Private ReadOnly Property Item(index As Integer) As Integer
        Get
            Return 42
        End Get
    End Property
End Class

Class ClassWithDelegate
    Delegate Sub MethodDelegate()
End Class

Class ClassWithEvent
    Private Event CustomEvent As EventHandler
End Class

Class OuterClass
    Class InnerEmpty1                               ' Noncompliant
    End Class

    Private Class InnerEmpty2                       ' Noncompliant
    End Class

    Protected Class InnerEmpty3                     ' Noncompliant
    End Class

    Class InnerEmpty4                               ' Noncompliant
    End Class

    Protected Class InnerEmpty5                     ' Noncompliant
    End Class

    Public Class InnerEmpty6                        ' Noncompliant
    End Class

    Public Class InnerEmptyWithComments             ' Noncompliant
        ' Some comment
    End Class

    Class InnerNonEmpty
        Public ReadOnly Property SomeProperty As Integer
            Get
                Return 42
            End Get
        End Property
    End Class
End Class

Class GenericEmpty(Of T)
    ' ^^^^^^^^^^^^
End Class

Class GenericEmptyWithConstraints(Of T As Class)    ' Noncompliant
End Class

Class GenericNotEmpty(Of T)
    Private Sub Method(arg As T)
    End Sub
End Class

Class GenericNotEmptyWithConstraints(Of T As Class)
    Private Sub Method(arg As T)
    End Sub
End Class

MustInherit Class AbstractEmpty                     ' Noncompliant
End Class

Partial Class PartialEmpty                          ' Compliant - Source Generators and some frameworks use empty partial classes as placeholders
End Class

Partial Class PartialNotEmpty
    Public ReadOnly Property Prop As Integer
        Get
            Return 42
        End Get
    End Property
End Class

Interface IMarker                                   ' Compliant - this rule only deals with classes
End Interface

Structure EmptyStruct                               ' Compliant - this rule only deals with classes
End Structure

Class                                               ' Error
End Class                                    
