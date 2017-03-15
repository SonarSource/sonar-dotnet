Namespace Tests.Diagnostics

    Public Class PropertyGetterWithThrow
        Public Property Foo() As Integer
            Get
                Throw New NotImplementedException  ' Noncompliant {{Remove the exception throwing from this property getter, or refactor the property into a method.}}
'               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            End Get
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property
        Public ReadOnly Property Foo2() As Integer
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