using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Data.OracleClient;
using System.Data.SqlServerCe;
using MySql.Data.MySqlClient;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;

namespace Tests.Diagnostics
{
    class Program
    {
        private const string ConstantQuery = "";

        public void CompliantSqlCommands(SqlConnection connection, SqlTransaction transaction, string query)
        {
            SqlCommand command;
            command = new SqlCommand(); // Compliant
            command = new SqlCommand(""); // Compliant
            command = new SqlCommand(ConstantQuery); // Compliant
            command = new SqlCommand(query); // Compliant, we don't know anything about the parameter
            command = new SqlCommand(query, connection); // Compliant
            command = new SqlCommand("", connection); // Compliant, constant queries are safe
            command = new SqlCommand(query, connection, transaction); // Compliant
            command = new SqlCommand("", connection, transaction); // Compliant, constant queries are safe
            command = new SqlCommand(query, connection, transaction, SqlCommandColumnEncryptionSetting.Enabled); // Compliant
            command = new SqlCommand("", connection, transaction, SqlCommandColumnEncryptionSetting.Enabled); // Compliant, constant queries are safe

            command.CommandText = query; // Compliant, we don't know enough about the parameter
            command.CommandText = ConstantQuery; // Compliant
            string text;
            text = command.CommandText; // Compliant
            text = command.CommandText = query; // Compliant

            SqlDataAdapter adapter;
            adapter = new SqlDataAdapter(); // Compliant
            adapter = new SqlDataAdapter(command); // Compliant
            adapter = new SqlDataAdapter(query, ""); // Compliant
            adapter = new SqlDataAdapter(query, connection); // Compliant
        }

        public void NonCompliant_Concat_SqlCommands(SqlConnection connection, SqlTransaction transaction, string query, string param)
        {
            var command = new SqlCommand(string.Concat(query, param)); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
//                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            command = new SqlCommand(query + param, connection); // Noncompliant
            command = new SqlCommand("" + 1 + 2, connection); // Compliant
            command = new SqlCommand(string.Concat(query, param), connection); // Noncompliant
            command = new SqlCommand(string.Concat(query, param), connection, transaction); // Noncompliant
            command = new SqlCommand(string.Concat(query, param), connection, transaction, SqlCommandColumnEncryptionSetting.Enabled); // Noncompliant

            command.CommandText = string.Concat(query, param); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^
            string text = command.CommandText = string.Concat(query, param); // Noncompliant

            var adapter = new SqlDataAdapter(string.Concat(query, param), ""); // Noncompliant
        }

        public void NonCompliant_Format_SqlCommands(SqlConnection connection, SqlTransaction transaction, string param)
        {
            var command = new SqlCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param)); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
//                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            command = new SqlCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param), connection); // Noncompliant
            command = new SqlCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param), connection, transaction); // Noncompliant
            command = new SqlCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param), connection, transaction, SqlCommandColumnEncryptionSetting.Enabled); // Noncompliant
            int x = 1;
            Guid g = Guid.NewGuid();
            DateTime dateTime = DateTime.Now;
            command = new SqlCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\", \"{2}\", \"{3}\")", x, g, dateTime), // Noncompliant - scalars can be dangerous and lead to expensive queries
                connection, transaction, SqlCommandColumnEncryptionSetting.Enabled);
            command = new SqlCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\", \"{2}\", \"{3}\")", x, param, dateTime), // Noncompliant
                connection, transaction, SqlCommandColumnEncryptionSetting.Enabled);

            command.CommandText = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param); // Noncompliant
            string text = command.CommandText = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param); // Noncompliant

            var adapter = new SqlDataAdapter(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param), ""); // Noncompliant
        }

        public void NonCompliant_Interpolation_SqlCommands(SqlConnection connection, SqlTransaction transaction, string param)
        {
            var command = new SqlCommand($"SELECT * FROM mytable WHERE mycol={param}"); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
//                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            command = new SqlCommand($"SELECT * FROM mytable WHERE mycol={param}", connection, transaction, SqlCommandColumnEncryptionSetting.Enabled); // Noncompliant

            command.CommandText = $"SELECT * FROM mytable WHERE mycol={param}"; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^

            var adapter = new SqlDataAdapter($"SELECT * FROM mytable WHERE mycol={param}", ""); // Noncompliant
        }

        public void OdbcCommands(OdbcConnection connection, OdbcTransaction transaction, string query)
        {
            OdbcCommand command;
            command = new OdbcCommand(); // Compliant
            command = new OdbcCommand(""); // Compliant
            command = new OdbcCommand(ConstantQuery); // Compliant
            command = new OdbcCommand(query); // Compliant, we don't know anything about the parameter
            command = new OdbcCommand(query, connection); // Compliant
            command = new OdbcCommand(query, connection, transaction); // Compliant

            command.CommandText = query; // Compliant
            command.CommandText = ConstantQuery; // Compliant
            string text;
            text = command.CommandText; // Compliant
            text = command.CommandText = query; // Compliant

            OdbcDataAdapter adapter;
            adapter = new OdbcDataAdapter(); // Compliant
            adapter = new OdbcDataAdapter(command); // Compliant
            adapter = new OdbcDataAdapter(query, $"concatenated connection string {query}"); // Compliant
            adapter = new OdbcDataAdapter(query, connection); // Compliant
        }

        /**
         * For the rest of the frameworks, we do sparse testing, to keep tests maintainable and relevant
         */

        public void NonCompliant_OdbcCommands(SqlConnection connection, SqlTransaction transaction, string query, string param)
        {
            var command = new OdbcCommand(string.Concat(query, param)); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
            command.CommandText = string.Concat(query, param); // Noncompliant
            command.CommandText = $"SELECT * FROM mytable WHERE mycol={param}"; // Noncompliant
            var adapter = new OdbcDataAdapter(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param), ""); // Noncompliant
        }

        public void OracleCommands(OracleConnection connection, OracleTransaction transaction, string query)
        {
            OracleCommand command;
            command = new OracleCommand(); // Compliant
            command = new OracleCommand(""); // Compliant
            command = new OracleCommand(ConstantQuery); // Compliant
            command = new OracleCommand(query); // Compliant, we don't know anything about the parameter
            command = new OracleCommand(query, connection); // Compliant, we don't know anything about the parameter
            command = new OracleCommand(query, connection, transaction); // Compliant, we don't know anything about the parameter

            command.CommandText = query; // Compliant, we don't know anything about the parameter
            command.CommandText = ConstantQuery; // Compliant
            string text;
            text = command.CommandText; // Compliant
            text = command.CommandText = query; // Compliant, we don't know anything about the parameter

            OracleDataAdapter adapter;
            adapter = new OracleDataAdapter(); // Compliant
            adapter = new OracleDataAdapter(command); // Compliant
            adapter = new OracleDataAdapter(query, $"nonconcatenated connection string {query}"); // Compliant, we don't know anything about the parameter
            adapter = new OracleDataAdapter(query, connection); // Compliant, we don't know anything about the parameter
        }

        public void NonCompliant_OracleCommands(OracleConnection connection, OracleTransaction transaction, string query, string param)
        {
            var command = new OracleCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param)); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
            command.CommandText = $"SELECT * FROM mytable WHERE mycol={param}"; // Noncompliant
            new OracleDataAdapter(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param), ""); // Noncompliant
        }

        public void SqlServerCeCommands(SqlCeConnection connection, SqlCeTransaction transaction, string query)
        {
            SqlCeCommand command;
            command = new SqlCeCommand(); // Compliant
            command = new SqlCeCommand(""); // Compliant
            command = new SqlCeCommand(ConstantQuery); // Compliant
            command = new SqlCeCommand(query); // Compliant
            command = new SqlCeCommand(query, connection); // Compliant
            command = new SqlCeCommand(query, connection, transaction); // Compliant

            command.CommandText = query; // Compliant
            command.CommandText = ConstantQuery; // Compliant
            string text;
            text = command.CommandText; // Compliant
            text = command.CommandText = query; // Compliant

            SqlCeDataAdapter adapter;
            adapter = new SqlCeDataAdapter(); // Compliant
            adapter = new SqlCeDataAdapter(command); // Compliant
            adapter = new SqlCeDataAdapter(query, string.Concat("concatenated connection string", query)); // Compliant
            adapter = new SqlCeDataAdapter(query, connection); // Compliant
        }

        public void NonCompliant_SqlCeCommands(SqlCeConnection connection, SqlCeTransaction transaction, string query, string param)
        {
            new SqlCeDataAdapter(string.Concat(query, param), ""); // Noncompliant
            var command = new SqlCeCommand($"SELECT * FROM mytable WHERE mycol={param}"); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
            command.CommandText = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param); // Noncompliant
        }

        public void MySqlDataCompliant(MySqlConnection connection, MySqlTransaction transaction, string query)
        {
            MySqlCommand command;
            command = new MySqlCommand();                                               // Compliant
            command = new MySqlCommand("");                                             // Compliant
            command = new MySqlCommand(query, connection, transaction);                 // Compliant

            command.CommandText = query;                                                // Compliant
            command.CommandText = ConstantQuery;                                        // Compliant
            string text;
            text = command.CommandText;                                                 // Compliant
            text = command.CommandText = query;                                         // Compliant

            var adapter = new MySqlDataAdapter("", connection);                         // Compliant
            adapter = new MySqlDataAdapter(ConstantQuery, "connectionString");          // Compliant

            MySqlHelper.ExecuteDataRow($"concatenated connection string = {query}", ConstantQuery);   // Compliant
        }

        public void NonCompliant_MySqlData(MySqlConnection connection, MySqlTransaction transaction, string query, string param)
        {
            var command = new MySqlCommand($"SELECT * FROM mytable WHERE mycol={param}");                          // Noncompliant
            command = new MySqlCommand($"SELECT * FROM mytable WHERE mycol={param}", connection);                  // Noncompliant
            command = new MySqlCommand($"SELECT * FROM mytable WHERE mycol={param}", connection, transaction);     // Noncompliant
            command.CommandText = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param);               // Noncompliant

            var adapter = new MySqlDataAdapter($"SELECT * FROM mytable WHERE mycol=" + param, connection);         // Noncompliant

            MySqlHelper.ExecuteDataRow("connectionString", $"SELECT * FROM mytable WHERE mycol={param}");          // Noncompliant
            MySqlHelper.ExecuteDataRowAsync("connectionString", $"SELECT * FROM mytable WHERE mycol={param}");     // Noncompliant
            MySqlHelper.ExecuteDataRowAsync("connectionString", $"SELECT * FROM mytable WHERE mycol={param}", new System.Threading.CancellationToken());    // Noncompliant
            MySqlHelper.ExecuteDataset("connectionString", $"SELECT * FROM mytable WHERE mycol={param}");          // Noncompliant
            MySqlHelper.ExecuteDatasetAsync("connectionString", $"SELECT * FROM mytable WHERE mycol={param}");     // Noncompliant
            MySqlHelper.ExecuteNonQuery("connectionString", $"SELECT * FROM mytable WHERE mycol={param}");         // Noncompliant
            MySqlHelper.ExecuteNonQueryAsync(connection, $"SELECT * FROM mytable WHERE mycol={param}");            // Noncompliant
            MySqlHelper.ExecuteReader(connection, $"SELECT * FROM mytable WHERE mycol={param}");                   // Noncompliant
            MySqlHelper.ExecuteReaderAsync(connection, $"SELECT * FROM mytable WHERE mycol={param}");              // Noncompliant
            MySqlHelper.ExecuteScalar(connection, $"SELECT * FROM mytable WHERE mycol={param}");                   // Noncompliant
            MySqlHelper.ExecuteScalarAsync(connection, $"SELECT * FROM mytable WHERE mycol={param}");              // Noncompliant
            MySqlHelper.UpdateDataSet("connectionString", $"SELECT * FROM mytable WHERE mycol={param}", new DataSet(), "tableName");                        // Noncompliant
            MySqlHelper.UpdateDataSetAsync("connectionString", $"SELECT * FROM mytable WHERE mycol={param}", new DataSet(), "tableName");                   // Noncompliant

            var script = new MySqlScript($"SELECT * FROM mytable WHERE mycol={param}");                            // Noncompliant
            script = new MySqlScript(connection, $"SELECT * FROM mytable WHERE mycol={param}");                    // Noncompliant
        }

        public void MicrosoftDataSqliteCompliant(SqliteConnection connection, string query)
        {
            SqliteCommand command;
            command = new SqliteCommand();          // Compliant
            command = new SqliteCommand("");        // Compliant
            command.CommandText = ConstantQuery;    // Compliant

            command.CommandText = query;            // Compliant
        }

        public void NonCompliant_MicrosoftDataSqlite(SqliteConnection connection, string query, string param)
        {
            var command = new SqliteCommand($"SELECT * FROM mytable WHERE mycol={param}", connection);  // Noncompliant
            command.CommandText = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param);    // Noncompliant
        }

        public void SystemDataSqliteCompliant(SQLiteConnection connection, SQLiteTransaction transaction, string query)
        {
            SQLiteCommand command;
            command = new SQLiteCommand();                                  // Compliant
            command = new SQLiteCommand("");                                // Compliant
            command = new SQLiteCommand(query, connection, transaction);    // Compliant

            command.CommandText = query;                                    // Compliant
            command.CommandText = ConstantQuery;                            // Compliant
            string text;
            text = command.CommandText;                                     // Compliant
            text = command.CommandText = query;                             // Compliant

            SQLiteCommand.Execute("SELECT * FROM mytable WHERE mycol={param}", SQLiteExecuteType.None, $"connectionString={query}");    // Compliant
        }

        public void NonCompliant_SystemDataSqlite(SQLiteConnection connection, SQLiteTransaction transaction, string query, string param)
        {
            var command = new SQLiteCommand($"SELECT * FROM mytable WHERE mycol={param}");                                      // Noncompliant
            command = new SQLiteCommand($"SELECT * FROM mytable WHERE mycol={param}", connection);                              // Noncompliant
            command = new SQLiteCommand($"SELECT * FROM mytable WHERE mycol={param}", connection, transaction);                 // Noncompliant
            command.CommandText = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param);                            // Noncompliant

            var adapter = new SQLiteDataAdapter($"SELECT * FROM mytable WHERE mycol=" + param, connection);                     // Noncompliant
            SQLiteCommand.Execute($"SELECT * FROM mytable WHERE mycol={param}", SQLiteExecuteType.None, "connectionString");    // Noncompliant
        }

        public void ConcatAndStringFormat(SqlConnection connection, string param)
        {
            SqlCommand command;
            string sensitiveQuery = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param);  // Secondary [1,2,3,4,5] {{SQL Query is dynamically formatted and assigned to sensitiveQuery.}}
            //     ^^^^^^^^^^^^^^
            command = new SqlCommand(sensitiveQuery);                                                   // Noncompliant [1]

            command.CommandText = sensitiveQuery;                                                       // Noncompliant [2]

            string stillSensitive = sensitiveQuery;                                                     // Secondary    ^20#14 [3] {{SQL query is assigned to stillSensitive.}}
            command.CommandText = stillSensitive;                                                       // Noncompliant ^13#19 [3]

            string sensitiveConcatQuery = "SELECT * FROM Table1 WHERE col1 = '" + param + "'";          // Secondary [6,7,8] {{SQL Query is dynamically formatted and assigned to sensitiveConcatQuery.}}

            command = new SqlCommand(sensitiveConcatQuery);                                             // Noncompliant [6]

            command.CommandText = sensitiveConcatQuery;                                                 // Noncompliant [7]

            string stillSensitiveConcat = sensitiveConcatQuery;                                         // Secondary [8] {{SQL query is assigned to stillSensitiveConcat.}}
            command.CommandText = stillSensitiveConcat;                                                 // Noncompliant [8]

            SqlDataAdapter adapter;
            adapter = new SqlDataAdapter(sensitiveQuery, connection);                                   // Noncompliant [4]

            command = new SqlCommand("SELECT * FROM Table1 WHERE col1 = '" + param + "'");              // Noncompliant
            command.CommandText = "SELECT * FROM Table1 WHERE col1 = '" + param + "'";                  // Noncompliant

            string x = null;
            x = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param);                      // Secondary ^13#1 [9] {{SQL Query is dynamically formatted and assigned to x.}}
            command.CommandText = x;                                                                    // Noncompliant    [9]

            string y;
            y = sensitiveQuery;                                                                         // Secondary ^13#1 [5] {{SQL query is assigned to y.}}
            command.CommandText = y;                                                                    // Noncompliant    [5]
        }

        public void DbCommand_CommandText(DbCommand command, string param)
        {
            command.CommandText = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param);    // Noncompliant
        }

        public void IDbCommand_CommandText(IDbCommand command, string param)
        {
            command.CommandText = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param);    // Noncompliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9602
    class Repro_9602
    {
        public void ConstantQuery(SqliteConnection connection, bool onlyEnabled)
        {
            string query = "SELECT id FROM users";
            if(onlyEnabled)
                query += " WHERE enabled = 1";
            string query2 = $"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"; // Secondary [c2, c3]

            var command = new SqliteCommand(query, connection);  // Compliant
            command.CommandText = query;    // Compliant

            var command2 = new SqliteCommand(query2, connection);  // Noncompliant [c2] - FP
            command2.CommandText = query2;    // Noncompliant [c3] - FP

            var command3 = new SqliteCommand($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}", connection);  // Noncompliant - FP
            command3.CommandText = $"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}";    // Noncompliant - FP
        }
    }
}
