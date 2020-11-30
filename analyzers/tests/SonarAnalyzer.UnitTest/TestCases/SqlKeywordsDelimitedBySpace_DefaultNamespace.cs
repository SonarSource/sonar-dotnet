using System.Data.SqlClient;

class Foo
{
    private string field = "SELECT *" + // FN
        "FROM TABLE" +
        "WHERE X = 1;";
}
