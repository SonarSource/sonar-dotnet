Imports System
Imports AliasName = System.Exception

Public Class Sample

    Private Field As Integer

    Public Sub New()
    End Sub

    Public Sub Go(Value As Integer)
        Dim G As New Generic(Of Integer)
        Dim A As New AliasName
        Value = 42
    End Sub

    Public Sub ValueAsVariableForCoverage()
        Dim Value As String = Nothing
    End Sub

    Public Property Prop As Integer
        Get
            Return Field
        End Get
        Set(Value As Integer)
            Field = Value
        End Set
    End Property

End Class

Public Class Generic(Of ItemType)
End Class
