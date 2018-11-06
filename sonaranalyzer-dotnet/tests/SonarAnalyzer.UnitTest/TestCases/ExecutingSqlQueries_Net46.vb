Imports System
Imports System.Data.SqlClient
Imports System.Data.Odbc
Imports System.Data.OracleClient
Imports System.Data.SqlServerCe

Namespace Tests.Diagnostics
    Class Program
        Private Const ConstantQuery As String = ""

        Public Sub SqlCommands(ByVal connection As SqlConnection, ByVal transaction As SqlTransaction, ByVal query As String)
            Dim command As SqlCommand
            command = New SqlCommand() ' Compliant
            command = New SqlCommand("") ' Compliant
            command = New SqlCommand(ConstantQuery) ' Compliant
            command = New SqlCommand(query) ' Noncompliant {{Make sure that executing SQL queries is safe here.}}
'                     ^^^^^^^^^^^^^^^^^^^^^
            command = New SqlCommand(query, connection) ' Noncompliant
            command = New SqlCommand("", connection) ' Compliant, constant queries are safe
            command = New SqlCommand(query, connection, transaction) ' Noncompliant
            command = New SqlCommand("", connection, transaction) ' Compliant, constant queries are safe
            command = New SqlCommand(query, connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Noncompliant
            command = New SqlCommand("", connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Compliant, constant queries are safe

            command.CommandText = query ' Noncompliant
'           ^^^^^^^^^^^^^^^^^^^
            command.CommandText = ConstantQuery ' Compliant
            Dim text As String
            text = command.CommandText ' Compliant
            Dim adapter As SqlDataAdapter
            adapter = New SqlDataAdapter() ' Compliant
            adapter = New SqlDataAdapter(command) ' Compliant
            adapter = New SqlDataAdapter(query, "") ' Noncompliant
            adapter = New SqlDataAdapter(query, connection) ' Noncompliant
        End Sub

        Public Sub OdbcCommands(ByVal connection As OdbcConnection, ByVal transaction As OdbcTransaction, ByVal query As String)
            Dim command As OdbcCommand
            command = New OdbcCommand() ' Compliant
            command = New OdbcCommand("") ' Compliant
            command = New OdbcCommand(ConstantQuery) ' Compliant
            command = New OdbcCommand(query) ' Noncompliant
            command = New OdbcCommand(query, connection) ' Noncompliant
            command = New OdbcCommand(query, connection, transaction) ' Noncompliant
            command.CommandText = query ' Noncompliant
            command.CommandText = ConstantQuery ' Compliant
            Dim text As String
            text = command.CommandText ' Compliant
            Dim adapter As OdbcDataAdapter
            adapter = New OdbcDataAdapter() ' Compliant
            adapter = New OdbcDataAdapter(command) ' Compliant
            adapter = New OdbcDataAdapter(query, "") ' Noncompliant
            adapter = New OdbcDataAdapter(query, connection) ' Noncompliant
        End Sub

        Public Sub OracleCommands(ByVal connection As OracleConnection, ByVal transaction As OracleTransaction, ByVal query As String)
            Dim command As OracleCommand
            command = New OracleCommand() ' Compliant
            command = New OracleCommand("") ' Compliant
            command = New OracleCommand(ConstantQuery) ' Compliant
            command = New OracleCommand(query) ' Noncompliant
            command = New OracleCommand(query, connection) ' Noncompliant
            command = New OracleCommand(query, connection, transaction) ' Noncompliant
            command.CommandText = query ' Noncompliant
            command.CommandText = ConstantQuery ' Compliant
            Dim text As String
            text = command.CommandText
            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter() ' Compliant
            adapter = New OracleDataAdapter(command) ' Compliant
            adapter = New OracleDataAdapter(query, "") ' Noncompliant
            adapter = New OracleDataAdapter(query, connection) ' Noncompliant
        End Sub

        Public Sub SqlServerCeCommands(ByVal connection As SqlCeConnection, ByVal transaction As SqlCeTransaction, ByVal query As String)
            Dim command As SqlCeCommand
            command = New SqlCeCommand() ' Compliant
            command = New SqlCeCommand("") ' Compliant
            command = New SqlCeCommand(ConstantQuery) ' Compliant
            command = New SqlCeCommand(query) ' Noncompliant
            command = New SqlCeCommand(query, connection) ' Noncompliant
            command = New SqlCeCommand(query, connection, transaction) ' Noncompliant
            command.CommandText = query ' Noncompliant
            command.CommandText = ConstantQuery ' Compliant
            Dim text As String
            text = command.CommandText ' Compliant
            Dim adapter As SqlCeDataAdapter
            adapter = New SqlCeDataAdapter() ' Compliant
            adapter = New SqlCeDataAdapter(command) ' Compliant
            adapter = New SqlCeDataAdapter(query, "") ' Noncompliant
            adapter = New SqlCeDataAdapter(query, connection) ' Noncompliant
        End Sub
    End Class
End Namespace
