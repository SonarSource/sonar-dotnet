Module Module1
    ' Internal state
    Dim array = {"apple", "banana", "orange", "pineapple", "strawberry"}

    ReadOnly Property Foo As String() ' Noncompliant {{Refactor 'Foo' into a method, properties should not be based on arrays.}}
'                     ^^^
        Get
            Dim copy = array.Clone      ' Expensive call
            Return copy
        End Get
    End Property
    Property Foo2() As String() ' Noncompliant
        Get
            Dim copy = array.Clone      ' Expensive call
            Return copy
        End Get
        Set(value As String())

        End Set
    End Property
    Property Foo3() As String
        Get
            Return "aaa"
        End Get
        Set(value As String)

        End Set
    End Property
End Module