Namespace Tests.Diagnostics
    Module Module1
        Public Property foo As Integer   ' Noncompliant
        Public Property FooFoo As Integer

        ReadOnly Property foooo(ByVal index As Integer) ' Noncompliant
            Get
                Return array(index)
            End Get
        End Property
    End Module
End Namespace