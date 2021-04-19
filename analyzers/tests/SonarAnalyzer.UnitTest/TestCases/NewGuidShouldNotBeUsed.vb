Imports System

Public Class NewGuid

    Public Sub WithoutArguments()

        Dim id As Guid = New Guid()  ' Noncompliant {{Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this Guid instantiation.}}
        '                ^^^^^^^^^^
    End Sub

    Public Sub WithArguments()
        Dim id As Guid = New Guid(New Byte() {}) ' Compliant
    End Sub

    Public Sub Other()
        Dim empty As Guid = Guid.Empty ' Compliant
        Dim rnd As Guid = Guid.NewGuid() ' Compliant
    End Sub

End Class
