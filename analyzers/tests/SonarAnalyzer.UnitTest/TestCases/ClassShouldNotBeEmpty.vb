
Class Empty                            ' Noncompliant {{Remove this empty class, or add members to it.}}
    ' ^^^^^
End Class


Public Class PublicEmpty               ' Noncompliant
End Class

Class InternalEmpty                    ' Noncompliant
End Class

Class EmptyWithComments                ' Noncompliant
    ' Some comment
End Class

Class NotEmpty
    Public ReadOnly Property SomeProperty As Integer
        Get
            Return 0
        End Get
    End Property
End Class

Class OuterClass
    Class InnerEmpty1                   ' Noncompliant
    End Class

    Private Class InnerEmpty2           ' Noncompliant
    End Class

    Protected Class InnerEmpty3         ' Noncompliant
    End Class

    Class InnerEmpty4                   ' Noncompliant
    End Class

    Protected Class InnerEmpty5         ' Noncompliant
    End Class

    Public Class InnerEmpty6            ' Noncompliant
    End Class

    Public Class InnerEmptyWithComments ' Noncompliant
        ' Some comment
    End Class

    Class InnerNonEmpty
        Public ReadOnly Property SomeProperty As Integer
            Get
                Return 0
            End Get
        End Property
    End Class
End Class

Partial Class PartialEmpty               ' Noncompliant
End Class

Partial Class PartialEmpty
    Public ReadOnly Property SomeProperty As Integer
        Get
            Return 0
        End Get
    End Property
End Class

Interface IMarker                        ' Compliant - this rule only deals with classes
End Interface

Structure EmptyStruct                    ' Compliant - this rule only deals with classes
End Structure

Class                                    ' Error
End Class                                    
