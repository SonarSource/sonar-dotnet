﻿Public Class IFoo ' NOSONAR

    Public Sub Method()
        Console.WriteLine("Hello, ")

        If Condition() Then
            Console.WriteLine("world!")
        End If
    End Sub

    Public Function Condition() As Boolean
        Return False
    End Function

End Class

Public Class ABCDEFGHIJKLMNOPQRSTUVWXYZ
    Public ReadOnly Property Prop As Integer
        Get
            Return 42
        End Get
    End Property
End Class
