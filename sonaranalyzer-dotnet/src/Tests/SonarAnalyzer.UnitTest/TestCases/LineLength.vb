Imports System
Imports System.Collections.Generic

Namespace Tests.Diagnostics

    Public Class LineLength
        Public Sub LineLength()

            Console.WriteLine("This is OK...") ' With these comments...........................................................
            Console.WriteLine() ' Noncompliant   {{Split this 128 characters long line (which is greater than 127 authorized).}}
        End Sub
    End Class
End Namespace

