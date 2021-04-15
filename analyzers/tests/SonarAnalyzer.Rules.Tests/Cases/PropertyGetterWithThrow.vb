Imports System
Namespace Tests.Diagnostics
    Public Class PropertyGetterWithThrow
        Public Property Foo1() As Integer
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
                Throw New NotImplementedException() ' Compliant
            End Get
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property
        Public Property Foo3() As Integer
            Get
                Throw New NotSupportedException() ' Compliant
            End Get
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property
        Public Property Foo4() As Integer
            Get
                Throw New PlatformNotSupportedException() ' Compliant
            End Get
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property
        Public ReadOnly Property Foo5() As Integer
            Get
                Return 1
            End Get
        End Property
        Public ReadOnly Property Foo6() As Integer
            Get
                Throw New InvalidOperationException() ' Compliant
            End Get
        End Property
        Public ReadOnly Property Foo7() As Integer
            Get
                Throw New ObjectDisposedException("foo") ' Compliant
            End Get
        End Property
        Public Property Item(ByVal index As Long) As Byte ' Indexed properties are ignored
            Get
                Throw New NotImplementedException
            End Get
            Set(ByVal Value As Byte)

            End Set
        End Property
    End Class
End Namespace
