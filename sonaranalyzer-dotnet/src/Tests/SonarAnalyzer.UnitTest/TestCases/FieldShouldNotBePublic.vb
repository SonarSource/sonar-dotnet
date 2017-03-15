Namespace Tests.TestCases
    Public Class FieldShouldNotBePublic
        Public Shared Pi = 3.14
        Public Const Pi2 = 3.14
        Public ReadOnly Pi3 As Double = 3.14 ' Noncompliant {{Make 'Pi3' private.}}
'                       ^^^
        Public pi7, ' Noncompliant
            Pi4 As Double = 3.14, ' Noncompliant
            Pi6 As Double = 3.14 ' Noncompliant
        Private Pi5 As Double = 3.14
    End Class
End Namespace