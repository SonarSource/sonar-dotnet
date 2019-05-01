using System;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Data.OracleClient;
using System.Data.SqlServerCe;

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
            var command = new SqlCommand(string.Concat(query, param)); // Noncompliant {{Make sure that executing SQL queries is safe here.}}
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
            var command = new SqlCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param)); // Noncompliant {{Make sure that executing SQL queries is safe here.}}
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
            var command = new SqlCommand($"SELECT * FROM mytable WHERE mycol={param}"); // Noncompliant {{Make sure that executing SQL queries is safe here.}}
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
            adapter = new OdbcDataAdapter(query, ""); // Compliant
            adapter = new OdbcDataAdapter(query, connection); // Compliant
        }

        /**
         * For the rest of the frameworks, we do sparse testing, to keep tests maintainable and relevant
         */

        public void NonCompliant_OdbcCommands(SqlConnection connection, SqlTransaction transaction, string query, string param)
        {
            var command = new OdbcCommand(string.Concat(query, param)); // Noncompliant {{Make sure that executing SQL queries is safe here.}}
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
            adapter = new OracleDataAdapter(query, ""); // Compliant, we don't know anything about the parameter
            adapter = new OracleDataAdapter(query, connection); // Compliant, we don't know anything about the parameter
        }

        public void NonCompliant_OracleCommands(OracleConnection connection, OracleTransaction transaction, string query, string param)
        {
            var command = new OracleCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param)); // Noncompliant {{Make sure that executing SQL queries is safe here.}}
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
            adapter = new SqlCeDataAdapter(query, ""); // Compliant
            adapter = new SqlCeDataAdapter(query, connection); // Compliant
        }

        public void NonCompliant_SqlCeCommands(SqlCeConnection connection, SqlCeTransaction transaction, string query, string param)
        {
            new SqlCeDataAdapter(string.Concat(query, param), ""); // Noncompliant
            var command = new SqlCeCommand($"SELECT * FROM mytable WHERE mycol={param}"); // Noncompliant {{Make sure that executing SQL queries is safe here.}}
            command.CommandText = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", param); // Noncompliant
        }
    }
}
