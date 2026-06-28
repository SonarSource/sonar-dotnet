using Oracle.ManagedDataAccess.Client;

namespace Tests.Diagnostics
{
    class OracleManagedDataAccessTest
    {
        public void OracleCommand_Compliant(OracleConnection connection, string query)
        {
            new OracleCommand("SELECT * FROM table");
            new OracleCommand("SELECT * FROM table", connection);

            var command = new OracleCommand();
            command.CommandText = "SELECT * FROM table";
        }

        public void OracleCommand_Noncompliant(OracleConnection connection, string userInput)
        {
            new OracleCommand($"SELECT * FROM table WHERE id = '{userInput}'"); // Noncompliant
            new OracleCommand($"SELECT * FROM table WHERE id = '{userInput}'", connection); // Noncompliant
            new OracleCommand("SELECT * FROM table WHERE id = '" + userInput + "'"); // Noncompliant
            new OracleCommand(string.Format("SELECT * FROM table WHERE id = '{0}'", userInput), connection); // Noncompliant
            new OracleCommand(string.Concat("SELECT * FROM table WHERE id = '", userInput, "'"), connection); // Noncompliant

            var command = new OracleCommand();
            command.CommandText = $"SELECT * FROM table WHERE id = '{userInput}'"; // Noncompliant
            command.CommandText = "SELECT * FROM table WHERE id = '" + userInput + "'"; // Noncompliant
            command.CommandText = string.Format("SELECT * FROM table WHERE id = '{0}'", userInput); // Noncompliant
            command.CommandText = string.Concat("SELECT * FROM table WHERE id = '", userInput, "'"); // Noncompliant
        }

        public void OracleCommand_VariableAssignment(OracleConnection connection, string userInput)
        {
            string query = $"SELECT * FROM table WHERE id = '{userInput}'"; // Secondary [1, 2, 3]
            new OracleCommand(query); // Noncompliant [1]
            new OracleCommand(query, connection); // Noncompliant [2]

            var command = new OracleCommand();
            command.CommandText = query; // Noncompliant [3]

            string constQuery = "SELECT * FROM table";
            new OracleCommand(constQuery);
            command.CommandText = constQuery;
        }

        public void OracleDataAdapter_Compliant(OracleConnection connection, string query)
        {
            new OracleDataAdapter("SELECT * FROM table", connection);
        }

        public void OracleDataAdapter_Noncompliant(OracleConnection connection, string userInput)
        {
            new OracleDataAdapter($"SELECT * FROM table WHERE id = '{userInput}'", connection); // Noncompliant
            new OracleDataAdapter("SELECT * FROM table WHERE id = '" + userInput + "'", connection); // Noncompliant
            new OracleDataAdapter(string.Format("SELECT * FROM table WHERE id = '{0}'", userInput), connection); // Noncompliant
        }

        public void OracleDataAdapter_VariableAssignment(OracleConnection connection, string userInput)
        {
            string query = $"SELECT * FROM table WHERE id = '{userInput}'"; // Secondary
            new OracleDataAdapter(query, connection); // Noncompliant

            string constQuery = "SELECT * FROM table";
            new OracleDataAdapter(constQuery, connection);
        }
    }
}

