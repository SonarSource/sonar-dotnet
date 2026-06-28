using Dapper;
using MySql.Data.MySqlClient;

// https://github.com/SonarSource/sonar-dotnet/issues/9602
class Repro_9602
{
    public void ConstantQuery(MySqlConnection db, bool onlyEnabled)
    {
        string query = "SELECT id FROM users";
        if(onlyEnabled)
            query += " WHERE enabled = 1";
        string query2 = $"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"; // Secondary

        db.Query<int>(query);                                                               // Compliant
        db.Query<int>(query2);                                                              // Noncompliant - FP
        db.Query<int>($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}");  // Noncompliant - FP
    }
}
