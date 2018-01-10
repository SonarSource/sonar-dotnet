using System.Data;

namespace Tests.Diagnostics
{
    class Program
    {
        void BadQueries(string name, string password)
        {
            var command1 = new System.Data.Odbc.OdbcCommand("SELECT AccountNumber FROM Users " + // Noncompliant {{Make sure to sanitize the parameters of this SQL command.}}
//                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            "WHERE Username='" + name +
            "' AND Password='" + password + "'");
            command1.CommandText = "SELECT AccountNumber FROM Users " + // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^
            "WHERE Username='" + name +
            "' AND Password='" + password + "'";

            var command2 = new System.Data.Odbc.OdbcDataAdapter("SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'", "");

            var command3 = new System.Data.OleDb.OleDbCommand("SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'");
            command3.CommandText = "SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'";

            var command4 = new System.Data.OleDb.OleDbDataAdapter("SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'", "");

            var command5 = new Oracle.ManagedDataAccess.Client.OracleCommand("SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'");
            command5.CommandText = "SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'";

            var command6 = new Oracle.ManagedDataAccess.Client.OracleDataAdapter("SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'", "");

            var command7 = new System.Data.SqlServerCe.SqlCeCommand("SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'");
            command7.CommandText = "SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'";

            var command8 = new System.Data.SqlServerCe.SqlCeDataAdapter("SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'", "");

            var command9 = new System.Data.SqlClient.SqlCommand("SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'");
            command9.CommandText = "SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'";

            var command10 = new System.Data.SqlClient.SqlDataAdapter("SELECT AccountNumber FROM Users " + // Noncompliant
            "WHERE Username='" + name +
            "' AND Password='" + password + "'", "");
        }

        void GoodQueries(string name, string password)
        {
            var goodCommand1 = new System.Data.Odbc.OdbcCommand("");
            goodCommand1.CommandText = "";

            var goodCommand2 = new System.Data.Odbc.OdbcCommand("SELECT AccountNumber From Users");
            goodCommand2.CommandText = "SELECT AccountNumber From Users";

            var goodCommand3 = new System.Data.Odbc.OdbcCommand();
            goodCommand3.Parameters.Add("@username", SqlDbType.NChar).Value = name;
            goodCommand3.Parameters.Add("@password", SqlDbType.NChar).Value = password;
            goodCommand3.CommandText = "SELECT AccountNumber FROM Users " +
               "WHERE Username=@username AND Password=@password";
        }
    }
}