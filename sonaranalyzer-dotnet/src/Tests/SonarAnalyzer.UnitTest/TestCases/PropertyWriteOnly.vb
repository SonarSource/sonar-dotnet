Namespace Tests.Diagnostics

    Public Class PropertyWriteOnly
        WriteOnly Property Foo() As Integer ' Noncompliant {{Provide a getter for 'Foo' or replace the property with a 'SetFoo' method.}}
'                          ^^^
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property

        Property Foo2() As Integer
            Get
                Return 1
            End Get
            Set(ByVal value As Integer)
                ' ... some code ...
            End Set
        End Property

    End Class
End Namespace