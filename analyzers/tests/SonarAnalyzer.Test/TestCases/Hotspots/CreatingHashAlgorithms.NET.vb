Imports System
Imports System.IO
Imports System.Security.Cryptography
Imports System.Threading.Tasks

Namespace Tests.Diagnostics

    Public Class Repro_FN_8758
        Public Sub Method()
            Dim data As Byte() = New Byte(41) {}
            Dim hashBuffer As Byte() = New Byte(SHA1.HashSizeInBytes - 1) {}
            Using stream As New MemoryStream(data)
                SHA1.HashData(stream)                                              ' Noncompliant
                SHA1.HashData(data)                                                ' Noncompliant
                Dim bytesWrittenSHA1 As Integer
                If SHA1.TryHashData(data, hashBuffer, bytesWrittenSHA1) Then       ' Noncompliant
                End If
                MD5.HashData(data)                                                 ' Noncompliant
                Dim bytesWrittenMD5 As Integer
                If MD5.TryHashData(data, hashBuffer, bytesWrittenMD5) Then         ' Noncompliant
                End If
            End Using
        End Sub

        Public Async Function Method2() As Task
            Using stream As New MemoryStream(New Byte(41) {})
                Await SHA1.HashDataAsync(stream)       ' Noncompliant
                Await MD5.HashDataAsync(stream)        ' Noncompliant
            End Using
        End Function

    End Class


End Namespace
