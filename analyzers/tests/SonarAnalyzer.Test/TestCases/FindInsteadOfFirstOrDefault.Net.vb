Imports System.Collections.Generic
Imports System.Linq
Imports System

Public Class FindInsteadOfFirstOrDefault
    Public Sub List(data As List(Of Integer))
        Dim unused = data.FirstOrDefault(Function(x) True)  ' Noncompliant
        unused = data.FirstOrDefault(0)                     ' Compliant
        unused = data.FirstOrDefault(Function(x) False, 0)  ' Compliant
    End Sub

    Public Sub Array(data As Integer())
        Dim unused = data.FirstOrDefault(Function(x) True)  ' Noncompliant
        unused = data.FirstOrDefault(0)                     ' Compliant
        unused = data.FirstOrDefault(Function(x) False, 0)  ' Compliant
    End Sub
End Class
