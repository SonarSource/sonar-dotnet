Module Module1
    ReadOnly Property Sum(ByVal a As Integer, ByVal b As Integer) ' Noncompliant {{This indexed property has 2 parameters, use methods instead.}}
'                     ^^^
        Get
            Return a + b
        End Get
    End Property

    ReadOnly Property Prop As Integer
        Get
            Return 42
        End Get
    End Property

    Function Sum3(ByVal a As Integer, ByVal b As Integer)          ' Compliant
        Return a + b
    End Function
End Module