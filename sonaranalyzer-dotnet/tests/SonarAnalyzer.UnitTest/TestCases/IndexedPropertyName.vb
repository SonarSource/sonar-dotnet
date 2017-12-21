Namespace Tests.Diagnostics
    Module Module1
        Public Property FooFoo As Integer
        Public Property FooFoo2() As Integer

        ReadOnly Property foooo(ByVal index As Integer) ' Noncompliant
            Get
                Return array(index)
            End Get
        End Property

        ReadOnly Property Item(ByVal index As Integer) ' Compliant
            Get
                Return array(index)
            End Get
        End Property
    End Module
End Namespace