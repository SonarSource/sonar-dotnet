Imports System

Public Class Sample

    Public Sub TryCatchWhenFinally()
        Try
            A()
            B()
            C()
        Catch Ex As Exception When Ex.Message.Contains("memory")
            Handle(Ex)
        Finally
            F()
        End Try
    End Sub

    Public Sub A()
    End Sub

    Public Sub B()
    End Sub

    Public Sub C()
    End Sub

    Public Sub F()
    End Sub

    Public Sub Handle(Ex As Exception)
    End Sub

End Class
