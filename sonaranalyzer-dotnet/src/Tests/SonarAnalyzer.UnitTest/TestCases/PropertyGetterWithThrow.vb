Imports System
Namespace Tests.Diagnostics
    Public Class PropertyGetterWithThrow
        Public Property Foo() As Integer
            Get
                Throw New Exception() ' Noncompliant
'               ^^^^^^^^^^^^^^^^^^^^^
            End Get
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property
        Public Property Foo2() As Integer
            Get
                Throw New NotImplementedException()
            End Get
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property
        Public Property Foo3() As Integer
            Get
                Throw New NotSupportedException()
            End Get
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property
        Public Property Foo3() As Integer
            Get
                Throw New PlatformNotSupportedException()
            End Get
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property
        Public ReadOnly Property Foo10() As Integer
            Get
                Return 1
            End Get
        End Property
        Public Property Item(ByVal index As Long) As Byte
            Get
                Throw New NotImplementedException
            End Get
            Set(ByVal Value As Byte)

            End Set
        End Property
    End Class
End Namespace