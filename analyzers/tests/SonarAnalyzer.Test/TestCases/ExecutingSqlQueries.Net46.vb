Imports System
Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.Odbc
Imports System.Data.OracleClient
Imports System.Data.SqlServerCe
Imports MySql.Data.MySqlClient
Imports MSSqlite = Microsoft.Data.Sqlite
Imports SystemSqlite = System.Data.SQLite

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
            Dim command = New SqlCommand(String.Concat(query, param)) ' Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
            '             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            command = New SqlCommand(query & param, connection) ' Noncompliant
            command = New SqlCommand(query + param, connection) ' Noncompliant
            command = New SqlCommand(query + param & param, connection) ' Noncompliant
            command = New SqlCommand(query & param + param, connection) ' Noncompliant
            command = New SqlCommand("" & 1 & 2, connection) ' Compliant, only constants
            command = New SqlCommand("" + 1 + 2, connection) ' Compliant, only constants
            command = New SqlCommand("" & 1 + 2, connection) ' Compliant, only constants
            command = New SqlCommand("" + 1 & 2, connection) ' Compliant, only constants
            command = New SqlCommand(String.Concat(query, param), connection) ' Noncompliant
            command = New SqlCommand(String.Concat(query, param), connection, transaction) ' Noncompliant
            command = New SqlCommand(String.Concat(query, param), connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Noncompliant
            Dim x As Integer = 1
            Dim g As Guid = Guid.NewGuid()
            Dim dateTime As DateTime = DateTime.Now
            command = New SqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"", ""{2}"", ""{3}"")", x, g, dateTime), connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Noncompliant - scalars can be dangerous and lead to expensive queries
            command = New SqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"", ""{2}"", ""{3}"")", x, param, dateTime), connection, transaction, SqlCommandColumnEncryptionSetting.Enabled) ' Noncompliant
            command.CommandText = String.Concat(query, param) ' Noncompliant ^13#19
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
            adapter = New OdbcDataAdapter(query, $"concatenated connection string {query}") ' Compliant
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
            adapter = New OracleDataAdapter(query, $"concatenated connection string {query}") ' Compliant
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
            adapter = New SqlCeDataAdapter(query, string.Concat("concatenated connection string", query)) ' Compliant
            adapter = New SqlCeDataAdapter(query, connection) ' Compliant
        End Sub

        Public Sub NonCompliant_SqlCeCommands(ByVal connection As SqlCeConnection, ByVal transaction As SqlCeTransaction, ByVal query As String, ByVal param As String)
            Dim x = New SqlCeDataAdapter(String.Concat(query, param), "") ' Noncompliant
            Dim command = New SqlCeCommand("" & param) ' Noncompliant
            command.CommandText = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param) ' Noncompliant
        End Sub

        Public Sub MySqlDataCompliant(ByVal connection As MySqlConnection, ByVal transaction As MySqlTransaction, ByVal query As String)
            Dim command As MySqlCommand
            command = New MySqlCommand()                                                ' Compliant
            command = New MySqlCommand("")                                              ' Compliant
            command = New MySqlCommand(query, connection, transaction)                  ' Compliant

            command.CommandText = query                                                 ' Compliant
            command.CommandText = ConstantQuery                                         ' Compliant
            Dim text As String
            text = command.CommandText                                                  ' Compliant

            Dim adapter As MySqlDataAdapter
            adapter = New MySqlDataAdapter("", connection)                              ' Compliant
            adapter = New MySqlDataAdapter(ConstantQuery, "connectionString")           ' Compliant

            MySqlHelper.ExecuteDataRow($"concatenated connection string = {query}", ConstantQuery)    ' Compliant
        End Sub

        Public Sub NonCompliant_MySqlData(ByVal connection As MySqlConnection, ByVal transaction As MySqlTransaction, ByVal query As String, ByVal param As String)
            Dim command As MySqlCommand
            command = New MySqlCommand($"SELECT * FROM mytable WHERE mycol={param}")                               ' Noncompliant
            command = New MySqlCommand($"SELECT * FROM mytable WHERE mycol={param}", connection)                   ' Noncompliant
            command = New MySqlCommand($"SELECT * FROM mytable WHERE mycol={param}", connection, transaction)      ' Noncompliant
            command.CommandText = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param)                ' Noncompliant

            Dim adapter As MySqlDataAdapter
            adapter = New MySqlDataAdapter($"SELECT * FROM mytable WHERE mycol=" + param, connection)              ' Noncompliant

            MySqlHelper.ExecuteDataRow("connectionString", $"SELECT * FROM mytable WHERE mycol={param}")           ' Noncompliant
            MySqlHelper.ExecuteDataRowAsync("connectionString", $"SELECT * FROM mytable WHERE mycol={param}")      ' Noncompliant
            MySqlHelper.ExecuteDataRowAsync("connectionString", $"SELECT * FROM mytable WHERE mycol={param}", New System.Threading.CancellationToken())     ' Noncompliant
            MySqlHelper.ExecuteDataset("connectionString", $"SELECT * FROM mytable WHERE mycol={param}")           ' Noncompliant
            MySqlHelper.ExecuteDatasetAsync("connectionString", $"SELECT * FROM mytable WHERE mycol={param}")      ' Noncompliant
            MySqlHelper.ExecuteNonQuery("connectionString", $"SELECT * FROM mytable WHERE mycol={param}")          ' Noncompliant
            MySqlHelper.ExecuteNonQueryAsync(connection, $"SELECT * FROM mytable WHERE mycol={param}")             ' Noncompliant
            MySqlHelper.ExecuteReader(connection, $"SELECT * FROM mytable WHERE mycol={param}")                    ' Noncompliant
            MySqlHelper.ExecuteReaderAsync(connection, $"SELECT * FROM mytable WHERE mycol={param}")               ' Noncompliant
            MySqlHelper.ExecuteScalar(connection, $"SELECT * FROM mytable WHERE mycol={param}")                    ' Noncompliant
            MySqlHelper.ExecuteScalarAsync(connection, $"SELECT * FROM mytable WHERE mycol={param}")               ' Noncompliant
            MySqlHelper.UpdateDataSet("connectionString", $"SELECT * FROM mytable WHERE mycol={param}", New DataSet(), "tableName")                         ' Noncompliant
            MySqlHelper.UpdateDataSetAsync("connectionString", $"SELECT * FROM mytable WHERE mycol={param}", New DataSet(), "tableName")                    ' Noncompliant

            Dim script As MySqlScript
            script = New MySqlScript($"SELECT * FROM mytable WHERE mycol={param}")                                 ' Noncompliant
            script = New MySqlScript(connection, $"SELECT * FROM mytable WHERE mycol={param}")                     ' Noncompliant
        End Sub

        Public Sub MicrosoftDataSqliteCompliant(ByVal connection As MSSqlite.SqliteConnection, ByVal query As String)
            Dim command As MSSqlite.SqliteCommand
            command = New MSSqlite.SqliteCommand()      ' Compliant
            command = New MSSqlite.SqliteCommand("")    ' Compliant
            command.CommandText = ConstantQuery         ' Compliant

            command.CommandText = query                 ' Compliant
        End Sub

        Public Sub NonCompliant_MicrosoftDataSqlite(ByVal connection As MSSqlite.SqliteConnection, ByVal query As String, ByVal param As String)
            Dim command As MSSqlite.SqliteCommand
            command = New MSSqlite.SqliteCommand($"SELECT * FROM mytable WHERE mycol={param}", connection)  ' Noncompliant
            command.CommandText = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param)         ' Noncompliant
        End Sub

        Public Sub SystemDataSqliteCompliant(ByVal connection As SystemSqlite.SQLiteConnection, ByVal transaction As SystemSqlite.SQLiteTransaction, ByVal query As String)
            Dim command As SystemSqlite.SQLiteCommand
            command = New SystemSqlite.SQLiteCommand()                                  ' Compliant
            command = New SystemSqlite.SQLiteCommand("")                                ' Compliant
            command = New SystemSqlite.SQLiteCommand(query, connection, transaction)    ' Compliant

            command.CommandText = query                                                 ' Compliant
            command.CommandText = ConstantQuery                                         ' Compliant
            Dim Text As String
            Text = command.CommandText                                                  ' Compliant
            Text = command.CommandText = query                                          ' Compliant
            SystemSqlite.SQLiteCommand.Execute("SELECT * FROM mytable WHERE mycol={param}", SystemSqlite.SQLiteExecuteType.None, $"connectionString={query}")   ' Compliant
        End Sub

        Public Sub NonCompliant_SystemDataSqlite(ByVal connection As SystemSqlite.SqliteConnection, ByVal transaction As SystemSqlite.SQLiteTransaction, ByVal query As String, ByVal param As String)
            Dim command As SystemSqlite.SQLiteCommand
            command = New SystemSqlite.SQLiteCommand($"SELECT * FROM mytable WHERE mycol={param}")                                                      ' Noncompliant
            command = New SystemSqlite.SQLiteCommand($"SELECT * FROM mytable WHERE mycol={param}", connection)                                          ' Noncompliant
            command = New SystemSqlite.SQLiteCommand($"SELECT * FROM mytable WHERE mycol={param}", connection, transaction)                             ' Noncompliant
            command.CommandText = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param)                                                     ' Noncompliant

            Dim adapter As SystemSqlite.SQLiteDataAdapter
            adapter = New SystemSqlite.SQLiteDataAdapter($"SELECT * FROM mytable WHERE mycol={param}", connection)                                      ' Noncompliant
            SystemSqlite.SQLiteCommand.Execute($"SELECT * FROM mytable WHERE mycol={param}", SystemSqlite.SQLiteExecuteType.None, "connectionString")   ' Noncompliant
        End Sub

        Public Sub ConcatAndStringFormat(ByVal connection As SqlConnection, ByVal param As String)
            Dim sensitiveQuery As String = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param)    ' Secondary [1,2,3,4] {{SQL Query is dynamically formatted and assigned to sensitiveQuery.}}
            '   ^^^^^^^^^^^^^^
            Dim command = New SqlCommand(sensitiveQuery)                                                        ' Noncompliant [1]
            command.CommandText = sensitiveQuery                                                                ' Noncompliant [2]

            Dim stillSensitive As String = sensitiveQuery                                                       ' Secondary [3] {{SQL query is assigned to stillSensitive.}}
            '   ^^^^^^^^^^^^^^
            command.CommandText = stillSensitive                                                                ' Noncompliant ^13#19 [3]

            Dim sensitiveConcatQuery As String = "SELECT * FROM Table1 WHERE col1 = '" + param + "'"            ' Secondary [5,6,7] {{SQL Query is dynamically formatted and assigned to sensitiveConcatQuery.}}

            command = New SqlCommand(sensitiveConcatQuery)                                                      ' Noncompliant [5]
            command.CommandText = sensitiveConcatQuery                                                          ' Noncompliant [6]

            Dim stillSensitiveConcat As String = sensitiveConcatQuery                                           ' Secondary    [7] {{SQL query is assigned to stillSensitiveConcat.}}
            command.CommandText = stillSensitiveConcat                                                          ' Noncompliant [7]

            Dim sensitiveConcatQuery2 As String = "SELECT * FROM Table1 WHERE col1 = '" & param & "'"           ' Secondary    [8,9,10] {{SQL Query is dynamically formatted and assigned to sensitiveConcatQuery2.}}
            command = New SqlCommand(sensitiveConcatQuery2)                                                     ' Noncompliant [8]
            command.CommandText = sensitiveConcatQuery2                                                         ' Noncompliant [9]

            Dim stillSensitiveConcat2 As String = sensitiveConcatQuery2                                         ' Secondary ^17#21 [10] {{SQL query is assigned to stillSensitiveConcat2.}}
            command.CommandText = stillSensitiveConcat2                                                         ' Noncompliant     [10]

            Dim x As String
            x = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", param)                               ' Secondary ^13#1 {{SQL Query is dynamically formatted and assigned to x.}}
            command.CommandText = x                                                                             ' Noncompliant

            Dim y As String
            y = sensitiveQuery                                                                                  ' Secondary ^13#1 [4] {{SQL query is assigned to y.}}
            command.CommandText = y                                                                             ' Noncompliant    [4]
        End Sub
    End Class

    ' https://github.com/SonarSource/sonar-dotnet/issues/9602
    Class Repro_9602
        Public Sub ConstantBuiltQuery(connection As MSSqlite.SqliteConnection, onlyEnabled As Boolean)
            Dim query As String = "SELECT id FROM users"
            If onlyEnabled Then
                query += " WHERE enabled = 1"
            End If
            Dim query2 As String = $"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}" ' Secondary [c2, c3]

            Dim command As New MSSqlite.SqliteCommand(query, connection)  ' Compliant
            command.CommandText = query    ' Compliant

            Dim command2 As New MSSqlite.SqliteCommand(query2, connection)  ' Noncompliant [c2] - FP
            command2.CommandText = query2    ' Noncompliant [c3] - FP

            Dim command3 As New MSSqlite.SqliteCommand($"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}", connection)  ' Noncompliant - FP
            command3.CommandText = $"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}"    ' Noncompliant - FP
        End Sub
    End Class
End Namespace
