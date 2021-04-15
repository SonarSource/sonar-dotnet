Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class TabCharacter

        Public Sub New()
            Dim tabs = "	" ' Noncompliant {{Replace all tab characters in this file by sequences of white-spaces.}}
            Dim tabs2 = "		"
            ' some more tabs: "		"
        End Sub
    End Class
End Namespace

