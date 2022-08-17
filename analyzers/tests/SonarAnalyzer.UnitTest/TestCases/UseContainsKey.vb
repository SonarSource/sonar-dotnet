Imports System.Collections.Generic
Imports System.Linq

Class Compliant
    Function UsesContainsKey() As Boolean
        Dim dict = New Dictionary(Of Integer, Integer) ' Compliant
        Return dict.ContainsKey(42)
    End Function

    Function KeysNotFollowedByContains() As Boolean
        Dim dict = New Dictionary(Of Integer, Integer) ' Compliant
        Return dict.Keys.Any()
    End Function

    Function KeysNotFollowedByAnything() As IEnumerable(Of Integer)
        Dim dict = New Dictionary(Of Integer, Integer) ' Compliant
        Return dict.Keys
    End Function

    Function NoDictionary() As Boolean
        Dim dict = New NoDictionary()
        Return dict.Keys.Contains(42) ' Compliant
    End Function
End Class

Class NonCompliant
    Function KeysContains() As Boolean
        Dim dict = New Dictionary(Of Integer, Integer) ' NonCompliant {{Use ContainsKey() instead.}}
        Return dict.Keys.Contains(42)
        '                ^^^^^^^^
    End Function
End Class

Class NoDictionary
    Public ReadOnly Property Keys As List(Of Integer)
End Class
