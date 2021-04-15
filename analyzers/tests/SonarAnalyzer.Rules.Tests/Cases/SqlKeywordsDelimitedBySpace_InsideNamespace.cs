namespace Tests.StaticUsing
{
    using static System.Data.SqlClient.SqlBulkCopyColumnMappingCollection;

    class Foo
    {
        private string field = "SELECT *" + // FN
            "FROM TABLE" +
            "WHERE X = 1;";
    }
}

namespace Tests.NoSqlNamespace
{
    class Foo
    {
        private string field = "SELECT *" + // FN
            "FROM TABLE" +
            "WHERE X = 1;";
    }
}

namespace Test.SqlAlias
{
    using SqlAlias = System.Data.SqlClient;
    class Foo
    {
        private string field = "SELECT a" +
            "FROM TABLE" + // Noncompliant
            " WHERE X = 1;";
    }
}

namespace Test.NormalUsing
{
    using System.Data.SqlClient;
    class Foo
    {
        private string field = "SELECT a" +
            "FROM TABLE" + // Noncompliant
            " WHERE X = 1;";
    }
}

