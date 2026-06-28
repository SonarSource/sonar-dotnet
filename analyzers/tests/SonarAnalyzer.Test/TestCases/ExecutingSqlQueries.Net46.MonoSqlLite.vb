Imports System
Imports System.Linq
Imports Mono.Data.Sqlite

Public Class Sample
    Private ConstQuery As String = ""

    Private Sub Compliant(ByVal connection As SqliteConnection)
        Dim command = New SqliteCommand()       ' Compliant
        command = New SqliteCommand(connection) ' Compliant
        Dim adapter = New SqliteDataAdapter()   ' Compliant
    End Sub

    Private Sub Foo(ByVal connection As SqliteConnection, transaction As SqliteTransaction, ByVal query As String, ParamArray parameters As Object())
        Dim command = New SqliteCommand($"SELECT * FROM mytable WHERE mycol={query}", connection)          ' Noncompliant
        command = New SqliteCommand($"SELECT * FROM mytable WHERE mycol={query}")                          ' Noncompliant
        command = New SqliteCommand($"SELECT * FROM mytable WHERE mycol={query}", connection, transaction) ' Noncompliant
        Dim adapter = New SqliteDataAdapter(String.Concat(query, parameters), connection)                  ' Noncompliant
        adapter = New SqliteDataAdapter(String.Concat(query, parameters), "connection")                    ' Noncompliant
    End Sub
End Class
