Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    ''' <summary>
    ''' FIXME: Do something ' Noncompliant
    ''' </summary>
    Class Foo
        Public Sub Test() ' FIXME
'                           ^^^^^

        End Sub
    End Class

    Class FixMe
        Sub FixMe()

        End Sub
    End Class
End Namespace 'fixme ' Noncompliant
