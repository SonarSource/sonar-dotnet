Imports System
Imports System.IO
Imports System.Security.Cryptography
Imports System.Threading
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


    ' https://sonarsource.atlassian.net/browse/NET-643
    Public Class CryptographicOperationsTests
        Public Sub CryptoOperations(data As Byte(), cancellationToken As CancellationToken)
            Using stream As New MemoryStream(data)
                CryptographicOperations.HashData(HashAlgorithmName.MD5, data)                                            ' Noncompliant
                CryptographicOperations.HashData(HashAlgorithmName.SHA1, data)                                           ' Noncompliant
                CryptographicOperations.HashData(HashAlgorithmName.SHA1, stream)                                         ' Noncompliant
                CryptographicOperations.HmacData(HashAlgorithmName.MD5, data, data)                                      ' Noncompliant
                CryptographicOperations.HmacData(HashAlgorithmName.SHA1, data, data)                                     ' Noncompliant
                CryptographicOperations.HmacData(HashAlgorithmName.MD5, data, stream)                                    ' Noncompliant

                Dim hashBuffer As Byte() = New Byte(31) {}
                Dim keyBuffer As Byte() = New Byte(15) {}
                Dim bytesWritten As Integer
                CryptographicOperations.TryHashData(HashAlgorithmName.MD5, data, hashBuffer, bytesWritten)               ' Noncompliant
                CryptographicOperations.TryHashData(HashAlgorithmName.SHA1, data, hashBuffer, bytesWritten)              ' Noncompliant
                CryptographicOperations.TryHmacData(HashAlgorithmName.MD5, keyBuffer, data, hashBuffer, bytesWritten)    ' Noncompliant
                CryptographicOperations.TryHmacData(HashAlgorithmName.SHA1, keyBuffer, data, hashBuffer, bytesWritten)   ' Noncompliant

                CryptographicOperations.HashData(HashAlgorithmName.SHA256, data)                                         ' Compliant
                CryptographicOperations.HmacData(HashAlgorithmName.SHA256, data, data)                                   ' Compliant
                CryptographicOperations.TryHashData(HashAlgorithmName.SHA3_256, data, hashBuffer, bytesWritten)          ' Compliant
                CryptographicOperations.TryHmacData(HashAlgorithmName.SHA3_256, keyBuffer, data, hashBuffer, bytesWritten) ' Compliant
            End Using
        End Sub

        Public Async Function CryptoOperationsAsync(data As Byte(), cancellationToken As CancellationToken) As Task
            Using stream As New MemoryStream(data)
                Await CryptographicOperations.HashDataAsync(HashAlgorithmName.MD5, stream, cancellationToken)             ' Noncompliant
                Await CryptographicOperations.HashDataAsync(HashAlgorithmName.SHA1, stream)                              ' Noncompliant
                Await CryptographicOperations.HmacDataAsync(HashAlgorithmName.MD5, data, stream, cancellationToken)      ' Noncompliant
                Await CryptographicOperations.HmacDataAsync(HashAlgorithmName.SHA1, data, stream)                        ' Noncompliant

                Await CryptographicOperations.HashDataAsync(HashAlgorithmName.SHA256, stream)                            ' Compliant
                Await CryptographicOperations.HmacDataAsync(HashAlgorithmName.SHA256, data, stream)                      ' Compliant
            End Using
        End Function
    End Class

End Namespace
