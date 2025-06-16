using Dapper;
using System.Data.SqlClient;

// https://github.com/SonarSource/sonar-dotnet/issues/9569
public class RepeatedParameterNamesInDatabase
{
    public void ExecuteSqlCommandsForUsers(SqlConnection connection)
    {
        var query = "SELECT * FROM Users WHERE Name = @name";
        var param = new DynamicParameters();
        param.Add("@name", "John Doe");                     // Noncompliant - FP: Name refers to parameters in different SQL tables.
        var result = connection.Query<User>(query, param);  // Renaming one does not necessitate renaming of parameters with the same name from other tables.
    }

    public void ExecuteSqlCommandsForCompanies(SqlConnection connection)
    {
        var query = "SELECT * FROM Companies WHERE Name = @name";
        var param = new DynamicParameters();
        param.Add("@name", "Constosco");                    // Secondary - FP
        var result = connection.Query<Company>(query, param);
    }

    public void ExecuteSqlCommandsForProducts(SqlConnection connection)
    {
        var query = "SELECT * FROM Companies WHERE Name = @name";
        var param = new DynamicParameters();
        param.Add("@name", "CleanBot 9000");                // Secondary - FP
        var result = connection.Query<Product>(query, param);
    }

    public void ExecuteSqlCommandsForCountries(SqlConnection connection)
    {
        var query = "SELECT * FROM Countries WHERE Name = @name";
        var param = new DynamicParameters();
        param.Add("@name", "Norway");                       // Secondary - FP
        var result = connection.Query<Country>(query, param);
    }

    public class Product { }
    public class Country { }
    public class Company { }
    public class User { }
}
