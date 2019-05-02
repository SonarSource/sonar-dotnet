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
            command = New SqlCommand(query) '  Compliant, not concat or format
            command = New SqlCommand(query, connection) ' Compliant, not concat or format
            command = New SqlCommand("", connection) ' Compliant, constant queries are safe
            command = New SqlCommand(query, connection, transaction) ' Compliant, not concat or format
            command = New SqlCommand("", connection, transaction) ' Compliant, constant queries are safe
            command = New SqlCommand(query, connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Compliant, not concat or format
            command = New SqlCommand("", connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Compliant, constant queries are safe

            command.CommandText = query ' Compliant, not concat or format
            command.CommandText = ConstantQuery ' Compliant
            Dim text As String
            text = command.CommandText ' Compliant
            Dim adapter As SqlDataAdapter
            adapter = New SqlDataAdapter() ' Compliant
            adapter = New SqlDataAdapter(command) ' Compliant
            adapter = New SqlDataAdapter(query, "") ' Compliant, not concat or format
            adapter = New SqlDataAdapter(query, connection) ' Compliant, not concat or format
        End Sub

        Public Sub NonCompliant_Concat_SqlCommands(ByVal connection As SqlConnection, ByVal transaction As SqlTransaction, ByVal query As String, ByVal param As String)
            Dim command = New SqlCommand(String.Concat(query, param)) ' Noncompliant {{Make sure that executing SQL queries is safe here.}}
'                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            command = New SqlCommand(query & param, connection) ' Noncompliant
            command = New SqlCommand("" & 1 & 2, connection) ' Compliant, only constants
            command = New SqlCommand(String.Concat(query, param), connection) ' Noncompliant
            command = New SqlCommand(String.Concat(query, param), connection, transaction) ' Noncompliant
            command = New SqlCommand(String.Concat(query, param), connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Noncompliant
            Dim x As Integer = 1
            Dim g As Guid = Guid.NewGuid()
            Dim dateTime As DateTime = DateTime.Now
            command = New SqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"", ""{2}"", ""{3}"")", x, g, dateTime), connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Noncompliant - scalars can be dangerous and lead to expensive queries
            command = New SqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"", ""{2}"", ""{3}"")", x, param, dateTime), connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Noncompliant
            command.CommandText = String.Concat(query, param) ' Noncompliant
'           ^^^^^^^^^^^^^^^^^^^
            Dim adapter = New SqlDataAdapter(String.Concat(query, param), "") ' Noncompliant
        End Sub

        Public Sub NonCompliant_Format_SqlCommands(ByVal connection As SqlConnection, ByVal transaction As SqlTransaction, ByVal param As String)
            Dim command = New SqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param)) ' Noncompliant
            command = New SqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param), connection) ' Noncompliant
            command = New SqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param), connection, transaction) ' Noncompliant
            command = New SqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param), connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Noncompliant
            command.CommandText = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param) ' Noncompliant
            Dim adapter = New SqlDataAdapter(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param), "") ' Noncompliant
        End Sub

        Public Sub NonCompliant_Interpolation_SqlCommands(ByVal connection As SqlConnection, ByVal transaction As SqlTransaction, ByVal param As String)
            Dim command = New SqlCommand("SELECT * FROM mytable WHERE mycol=" & param) ' Noncompliant
            command = New SqlCommand($"SELECT * FROM mytable WHERE mycol={param}", connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Noncompliant
            command.CommandText = "SELECT * FROM mytable WHERE mycol=" & param ' Noncompliant
            Dim adapter = New SqlDataAdapter("SELECT * FROM mytable WHERE mycol=" & param, "") ' Noncompliant
        End Sub

        Public Sub OdbcCommands(ByVal connection As OdbcConnection, ByVal transaction As OdbcTransaction, ByVal query As String)
            Dim command As OdbcCommand
            command = New OdbcCommand() ' Compliant
            command = New OdbcCommand("") ' Compliant
            command = New OdbcCommand(ConstantQuery) ' Compliant
            command = New OdbcCommand(query) ' Compliant
            command = New OdbcCommand(query, connection) ' Compliant
            command = New OdbcCommand(query, connection, transaction) ' Compliant
            command.CommandText = query ' Compliant
            command.CommandText = ConstantQuery ' Compliant
            Dim text As String
            text = command.CommandText ' Compliant
            Dim adapter As OdbcDataAdapter
            adapter = New OdbcDataAdapter() ' Compliant
            adapter = New OdbcDataAdapter(command) ' Compliant
            adapter = New OdbcDataAdapter(query, "") ' Compliant
            adapter = New OdbcDataAdapter(query, connection) ' Compliant
        End Sub

        Public Sub NonCompliant_OdbcCommands(ByVal connection As SqlConnection, ByVal transaction As SqlTransaction, ByVal query As String, ByVal param As String)
            Dim command = New OdbcCommand(String.Concat(query, param)) ' Noncompliant
            command.CommandText = String.Concat(query, param) ' Noncompliant
            command.CommandText = "SELECT * FROM mytable WHERE mycol=" & param ' Noncompliant
            command.CommandText = $"SELECT * FROM mytable WHERE mycol={param}" ' Noncompliant
            Dim adapter = New OdbcDataAdapter(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param), "") ' Noncompliant
            Dim adapter1 = New odbcdataadapter(sTRing.foRmAt("INSERT INTO Users (name) VALUES (""{0}"")", param), "") ' Noncompliant
        End Sub

        Public Sub OracleCommands(ByVal connection As OracleConnection, ByVal transaction As OracleTransaction, ByVal query As String)
            Dim command As OracleCommand
            command = New OracleCommand() ' Compliant
            command = New OracleCommand("") ' Compliant
            command = New OracleCommand(ConstantQuery) ' Compliant
            command = New OracleCommand(query) ' Compliant
            command = New OracleCommand(query, connection) ' Compliant
            command = New OracleCommand(query, connection, transaction) ' Compliant
            command.CommandText = query ' Compliant
            command.CommandText = ConstantQuery ' Compliant
            Dim text As String
            text = command.CommandText
            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter() ' Compliant
            adapter = New OracleDataAdapter(command) ' Compliant
            adapter = New OracleDataAdapter(query, "") ' Compliant
            adapter = New OracleDataAdapter(query, connection) ' Compliant
        End Sub

        Public Sub NonCompliant_OracleCommands(ByVal connection As OracleConnection, ByVal transaction As OracleTransaction, ByVal query As String, ByVal param As String)
            Dim command = New OracleCommand("SELECT * FROM mytable WHERE mycol=" & param) ' Noncompliant
            command.CommandText = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param) ' Noncompliant
            command.CommandText = string.forMAT("INSERT INTO Users (name) VALUES (""{0}"")", param) ' Noncompliant
            Dim x = New OracleDataAdapter(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param), "") ' Noncompliant
            x = New OracleDataAdapter($"INSERT INTO Users (name) VALUES (""{param}"")", "") ' Noncompliant
        End Sub

        Public Sub SqlServerCeCommands(ByVal connection As SqlCeConnection, ByVal transaction As SqlCeTransaction, ByVal query As String)
            Dim command As SqlCeCommand
            command = New SqlCeCommand() ' Compliant
            command = New SqlCeCommand("") ' Compliant
            command = New SqlCeCommand(ConstantQuery) ' Compliant
            command = New SqlCeCommand(query) ' Compliant
            command = New SqlCeCommand(query, connection) ' Compliant
            command = New SqlCeCommand(query, connection, transaction) ' Compliant
            command.CommandText = query ' Compliant
            command.CommandText = ConstantQuery ' Compliant
            Dim text As String
            text = command.CommandText ' Compliant
            Dim adapter As SqlCeDataAdapter
            adapter = New SqlCeDataAdapter() ' Compliant
            adapter = New SqlCeDataAdapter(command) ' Compliant
            adapter = New SqlCeDataAdapter(query, "") ' Compliant
            adapter = New SqlCeDataAdapter(query, connection) ' Compliant
        End Sub

        Public Sub NonCompliant_SqlCeCommands(ByVal connection As SqlCeConnection, ByVal transaction As SqlCeTransaction, ByVal query As String, ByVal param As String)
            Dim x = New SqlCeDataAdapter(String.Concat(query, param), "") ' Noncompliant
            Dim command = New SqlCeCommand("" & param) ' Noncompliant
            command.CommandText = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param) ' Noncompliant
        End Sub

    End Class
End Namespace
