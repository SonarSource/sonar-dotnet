Imports System
Imports System.Collections.Generic

Namespace Tests.Diagnostics

    Public Class LineLength
        Public Sub LineLength()

            Console.WriteLine("This is OK...") ' With these comments............................................................
            ' Noncompliant@-1 {{Split this 128 characters long line (which is greater than 127 authorized).}}
            Console.WriteLine()

            ' Next line is compliant because it contains the error pattern so we prevent the issue to be reported
            Console.WriteLine(" error: foo bar oeirpo worweoi rewopir epwooriewporiewpor iweporiw oriewporiewprowe ipoewirpewoirewporiwepo")
        End Sub
    End Class
End Namespace

