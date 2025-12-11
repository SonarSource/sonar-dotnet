using Microsoft.Data.SqlClient;

namespace Tests.Diagnostics
{
    class MicrosoftDataSqlClientTest
    {
        public void MicrosoftDataSqlClient_Compliant(SqlConnection connection, SqlTransaction transaction, string query)
        {
            // Compliant - constant query strings
            new SqlCommand("SELECT * FROM table");
            new SqlCommand("SELECT * FROM table", connection);
            new SqlCommand("SELECT * FROM table", connection, transaction);
            new SqlCommand("SELECT * FROM table", connection, transaction, SqlCommandColumnEncryptionSetting.Enabled);

            var command = new SqlCommand();
            command.CommandText = "SELECT * FROM table";

            new SqlDataAdapter("SELECT * FROM table", connection);
        }

        public void MicrosoftDataSqlClient_Noncompliant(SqlConnection connection, SqlTransaction transaction, string userInput)
        {
            new SqlCommand($"SELECT * FROM table WHERE id = '{userInput}'"); // Noncompliant
            new SqlCommand($"SELECT * FROM table WHERE id = '{userInput}'", connection); // Noncompliant
            new SqlCommand($"SELECT * FROM table WHERE id = '{userInput}'", connection, transaction); // Noncompliant
            new SqlCommand($"SELECT * FROM table WHERE id = '{userInput}'", connection, transaction, SqlCommandColumnEncryptionSetting.Enabled); // Noncompliant

            new SqlCommand("SELECT * FROM table WHERE id = '" + userInput + "'"); // Noncompliant
            new SqlCommand(string.Format("SELECT * FROM table WHERE id = '{0}'", userInput), connection); // Noncompliant
            new SqlCommand(string.Concat("SELECT * FROM table WHERE id = '", userInput, "'"), connection, transaction); // Noncompliant

            var command = new SqlCommand();
            command.CommandText = $"SELECT * FROM table WHERE id = '{userInput}'"; // Noncompliant
            command.CommandText = "SELECT * FROM table WHERE id = '" + userInput + "'"; // Noncompliant
            command.CommandText = string.Format("SELECT * FROM table WHERE id = '{0}'", userInput); // Noncompliant
            command.CommandText = string.Concat("SELECT * FROM table WHERE id = '", userInput, "'"); // Noncompliant

            new SqlDataAdapter($"SELECT * FROM table WHERE id = '{userInput}'", connection); // Noncompliant
            new SqlDataAdapter("SELECT * FROM table WHERE id = '" + userInput + "'", connection); // Noncompliant
            new SqlDataAdapter(string.Format("SELECT * FROM table WHERE id = '{0}'", userInput), connection); // Noncompliant
        }

        public void MicrosoftDataSqlClient_VariableAssignment(SqlConnection connection, string userInput)
        {
            string query = $"SELECT * FROM table WHERE id = '{userInput}'"; // Secondary [1, 2, 3, 4]
            new SqlCommand(query); // Noncompliant [1]
            new SqlCommand(query, connection); // Noncompliant [2]

            var command = new SqlCommand();
            command.CommandText = query; // Noncompliant [3]

            new SqlDataAdapter(query, connection); // Noncompliant [4]

            // Compliant - variable assigned with constant
            string constQuery = "SELECT * FROM table";
            new SqlCommand(constQuery);
            command.CommandText = constQuery;
        }
    }
}

