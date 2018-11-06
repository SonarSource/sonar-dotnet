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

        public void SqlCommands(SqlConnection connection, SqlTransaction transaction, string query)
        {
            SqlCommand command;
            command = new SqlCommand(); // Compliant
            command = new SqlCommand(""); // Compliant
            command = new SqlCommand(ConstantQuery); // Compliant
            command = new SqlCommand(query); // Noncompliant {{Make sure that executing SQL queries is safe here.}}
//                    ^^^^^^^^^^^^^^^^^^^^^
            command = new SqlCommand(query, connection); // Noncompliant
            command = new SqlCommand("", connection); // Compliant, constant queries are safe
            command = new SqlCommand(query, connection, transaction); // Noncompliant
            command = new SqlCommand("", connection, transaction); // Compliant, constant queries are safe
            command = new SqlCommand(query, connection, transaction, SqlCommandColumnEncryptionSetting.Enabled); // Noncompliant
            command = new SqlCommand("", connection, transaction, SqlCommandColumnEncryptionSetting.Enabled); // Compliant, constant queries are safe

            command.CommandText = query; // Noncompliant
            command.CommandText = ConstantQuery; // Compliant
            string text;
            text = command.CommandText; // Compliant
            text = command.CommandText = query; // Noncompliant
//                 ^^^^^^^^^^^^^^^^^^^

            SqlDataAdapter adapter;
            adapter = new SqlDataAdapter(); // Compliant
            adapter = new SqlDataAdapter(command); // Compliant
            adapter = new SqlDataAdapter(query, ""); // Noncompliant
            adapter = new SqlDataAdapter(query, connection); // Noncompliant
        }

        public void OdbcCommands(OdbcConnection connection, OdbcTransaction transaction, string query)
        {
            OdbcCommand command;
            command = new OdbcCommand(); // Compliant
            command = new OdbcCommand(""); // Compliant
            command = new OdbcCommand(ConstantQuery); // Compliant
            command = new OdbcCommand(query); // Noncompliant
            command = new OdbcCommand(query, connection); // Noncompliant
            command = new OdbcCommand(query, connection, transaction); // Noncompliant

            command.CommandText = query; // Noncompliant
            command.CommandText = ConstantQuery; // Compliant
            string text;
            text = command.CommandText; // Compliant
            text = command.CommandText = query; // Noncompliant

            OdbcDataAdapter adapter;
            adapter = new OdbcDataAdapter(); // Compliant
            adapter = new OdbcDataAdapter(command); // Compliant
            adapter = new OdbcDataAdapter(query, ""); // Noncompliant
            adapter = new OdbcDataAdapter(query, connection); // Noncompliant
        }

        public void OracleCommands(OracleConnection connection, OracleTransaction transaction, string query)
        {
            OracleCommand command;
            command = new OracleCommand(); // Compliant
            command = new OracleCommand(""); // Compliant
            command = new OracleCommand(ConstantQuery); // Compliant
            command = new OracleCommand(query); // Noncompliant
            command = new OracleCommand(query, connection); // Noncompliant
            command = new OracleCommand(query, connection, transaction); // Noncompliant

            command.CommandText = query; // Noncompliant
            command.CommandText = ConstantQuery; // Compliant
            string text;
            text = command.CommandText; // Compliant
            text = command.CommandText = query; // Noncompliant

            OracleDataAdapter adapter;
            adapter = new OracleDataAdapter(); // Compliant
            adapter = new OracleDataAdapter(command); // Compliant
            adapter = new OracleDataAdapter(query, ""); // Noncompliant
            adapter = new OracleDataAdapter(query, connection); // Noncompliant
        }

        public void SqlServerCeCommands(SqlCeConnection connection, SqlCeTransaction transaction, string query)
        {
            SqlCeCommand command;
            command = new SqlCeCommand(); // Compliant
            command = new SqlCeCommand(""); // Compliant
            command = new SqlCeCommand(ConstantQuery); // Compliant
            command = new SqlCeCommand(query); // Noncompliant
            command = new SqlCeCommand(query, connection); // Noncompliant
            command = new SqlCeCommand(query, connection, transaction); // Noncompliant

            command.CommandText = query; // Noncompliant
            command.CommandText = ConstantQuery; // Compliant
            string text;
            text = command.CommandText; // Compliant
            text = command.CommandText = query; // Noncompliant

            SqlCeDataAdapter adapter;
            adapter = new SqlCeDataAdapter(); // Compliant
            adapter = new SqlCeDataAdapter(command); // Compliant
            adapter = new SqlCeDataAdapter(query, ""); // Noncompliant
            adapter = new SqlCeDataAdapter(query, connection); // Noncompliant
        }
    }
}
