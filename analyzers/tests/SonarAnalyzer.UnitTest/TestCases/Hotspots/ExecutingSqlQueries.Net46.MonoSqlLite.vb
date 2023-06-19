Imports System
Imports System.Linq
Imports Mono.Data.Sqlite

Namespace Test
    Public Class C
        Private ConstQuery As String = ""

        Private Sub Compliant(ByVal connection As SqliteConnection)
            Dim command = New SqliteCommand()
            command = New SqliteCommand(connection)
            Dim adapter = New SqliteDataAdapter()
        End Sub

        Private Sub Foo(ByVal connection As SqliteConnection, ByVal query As String, ParamArray parameters As Object())
            Dim command = New SqliteCommand($"SELECT * FROM mytable WHERE mycol={query}", connection)
            command = New SqliteCommand($"SELECT * FROM mytable WHERE mycol={query}")
            Dim adapter = New SqliteDataAdapter(String.Concat(query, parameters), connection)
        End Sub
    End Class
End Namespace
