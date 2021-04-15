Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    ''' <summary>
    ''' TODO: Do something ' Noncompliant
    ''' </summary>
    Class Foo
        Public Sub Test() ' TODO ' Noncompliant
'                           ^^^^

        End Sub
    End Class

    Class Todo
        Sub Todo() ' this is not a ctor
        End Sub
    End Class
End Namespace 'todo ' Noncompliant
