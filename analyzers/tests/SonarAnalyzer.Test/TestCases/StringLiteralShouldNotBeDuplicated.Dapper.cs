using Dapper;
using System.Data.SqlClient;

// https://sonarsource.atlassian.net/browse/NET-1565
public class RepeatedParameterNamesInDatabase
{
    public void ExecuteSqlCommandsForUsers(SqlConnection connection)
    {
        var query = "SELECT * FROM Users WHERE Name = @name";
        var param = new DynamicParameters();
        param.Add("@name", "John Doe");                     // Compliant - Name refers to parameters in different SQL tables.
        var result = connection.Query<User>(query, param);  // Renaming one does not necessitate renaming of parameters with the same name from other tables.
    }

    public void ExecuteSqlCommandsForCompanies(SqlConnection connection)
    {
        var query = "SELECT * FROM Companies WHERE Name = @name";
        var param = new DynamicParameters();
        param.Add("@name", "Constosco");                    // Compliant
        var result = connection.Query<Company>(query, param);
    }

    public void ExecuteSqlCommandsForProducts(SqlConnection connection)
    {
        var query = "SELECT * FROM Companies WHERE Name = @name";
        var param = new DynamicParameters();
        param.Add("@name", "CleanBot 9000");                // Compliant
        var result = connection.Query<Product>(query, param);
    }

    public void ExecuteSqlCommandsForCountries(SqlConnection connection)
    {
        var query = "SELECT * FROM Countries WHERE Name = @name";
        var param = new DynamicParameters();
        param.Add("@name", "Norway");                       // Compliant
        var result = connection.Query<Country>(query, param);
    }

    public class Product { }
    public class Country { }
    public class Company { }
    public class User { }
}

public class FN
{
    // FN - "@name" is used as an exception parameter name, not as a Dapper SQL parameter.
    // The current implementation suppresses all @-prefixed strings when Dapper is present.
    public void A() => throw new System.ArgumentException("Invalid argument.", "@name"); // FN

    // FN - "@name" is used as a value argument to DynamicParameters.Add (not as the SQL parameter name).
    public void NameIsValueArgument(SqlConnection connection)
    {
        var param = new DynamicParameters();
        param.Add("@other", "@name");   // FN
        connection.Query<User>("SELECT * FROM Users WHERE Other = @other", param);
    }

    public void UsedInMethodSecondTime(SqlConnection connection)
    {
        var param = new DynamicParameters();
        param.Add("@other", "@name");   // FN
        connection.Query<Company>("SELECT * FROM Companies WHERE Other = @other", param);
    }

    public void UsedInMethodThirdTime(SqlConnection connection)
    {
        var param = new DynamicParameters();
        param.Add("@other", "@name");   // FN
        connection.Query<Product>("SELECT * FROM Products WHERE Other = @other", param);
    }

    public class User { }
    public class Company { }
    public class Product { }
}
