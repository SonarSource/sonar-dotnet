Imports System
Imports System.Collections.Generic

Public Class InsecureEncryptionAlgorithm

    Public Sub NullConditionalIndexing(List As List(Of Integer))
        Dim Value As Integer = List?(0)
    End Sub

End Class
