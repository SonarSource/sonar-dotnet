Imports System
Imports System.IO

Public Class TestCases
    ReadOnly Property TempFileName() As String
        Get
            TempFileName = Path.GetTempFileName() 'Noncompliant
            Exit Property
        End Get
    End Property

    Public Sub Method()
        Dim TempPath = Path.GetTempFileName() 'Noncompliant
    End Sub

    Public Function GetTempFileNameGenerator() As Func(Of String)
        Return AddressOf Path.GetTempFileName 'Noncompliant
    End Function

    Public Sub Test()
        Consume(AddressOf Path.GetTempFileName) 'Noncompliant
    End Sub

    Public Sub Consume(generator as Func(Of String))
        Dim path = generator()
    End Sub

    Public Function Compliant() As String
        Return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) 'Compliant
    End Function
End Class
