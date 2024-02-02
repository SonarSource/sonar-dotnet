Imports System
Imports System.Collections.Generic
Imports System.Runtime.InteropServices

Namespace Compliant
    Public Class PublicEmpty                            ' Noncompliant
    End Class

    Friend Class InternalEmpty                          ' Noncompliant
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

    Class GenericNotEmpty(Of T)
        Private Sub Method(arg As T)
        End Sub
    End Class

    Class GenericNotEmptyWithConstraints(Of T As Class)
        Private Sub Method(arg As T)
        End Sub
    End Class

    Class IntegerList                                   ' Compliant - creates a more specific type without adding new members to it (similar to using the typedef keyword in C/C++)
        Inherits List(Of Integer)
    End Class

    Class StringLookup(Of T)
        Inherits Dictionary(Of String, T)
    End Class

    Interface IIntegerSet(Of T)
        Inherits ISet(Of Integer)
    End Interface

    <ComVisible(True)>
    Class ClassWithAttribute                            ' Compliant - types with attributes are ignored
    End Class

    <ComVisible(True), Obsolete>
    Class ClassWithMultipleAttributes
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

    Class Conditional                                   ' Compliant - it's not empty when the given symbol is defined
#If NOTDEFINED Then
    Public Overrides Function ToString() As String
        Return "Debug Text"
    End Function
#End If
    End Class

End Namespace

Namespace NonCompliant

    Class Empty                                         ' Noncompliant {{Remove this empty class, write its code or make it an "interface".}}
        ' ^^^^^
    End Class

    Class OuterClass

        Class InnerEmpty1                               ' Noncompliant
        End Class

        Private Class InnerEmpty2                       ' Noncompliant
        End Class

        Protected Class InnerEmpty3                     ' Noncompliant
        End Class

        Friend Class InnerEmpty4                        ' Noncompliant
        End Class

        Protected Friend Class InnerEmpty5              ' Noncompliant
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

    Class GenericEmpty(Of T)                            ' Noncompliant
        ' ^^^^^^^^^^^^
    End Class

    Class GenericEmptyWithConstraints(Of T As Class)    ' Noncompliant
    End Class

    MustInherit Class AbstractEmpty                     ' Noncompliant
    End Class

End Namespace

Namespace Ignore

    Class                                               ' Error [BC30203]
    End Class

    <>                                                  ' Error [BC30203]
    Class AttributeError                                ' Noncompliant
    End Class

    Interface IMarker                                   ' Compliant - this rule only deals with classes
    End Interface

    Class ImplementsMarker                              ' Compliant - implements a marker interface
        Implements IMarker
    End Class

    Structure EmptyStruct                               ' Compliant - this rule only deals with classes
    End Structure

End Namespace
