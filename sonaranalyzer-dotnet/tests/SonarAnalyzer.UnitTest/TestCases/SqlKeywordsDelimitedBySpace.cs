using System;
using System.Data.SqlClient;
using Linq = System.Linq;

namespace Tests.Diagnostics
{
    // we don't look for objects from the SqlClient namespace, we just look at the namespace

    class NonCompliantExamples
    {
        public void VariousSqlKeywords()
        {
            var bulkInsert = "BULK INSERT AdventureWorks2012.Sales.SalesOrderDetail" +
               "FROM 'neworders.txt';"; // Noncompliant {{Add a space before 'FROM'.}}
//             ^^^^^^^^^^^^^^^^^^^^^^^
            var select = "SELECT e.*" +
                "FROM DimEmployee AS e" + // Noncompliant
                "ORDER BY LastName;"; // Noncompliant
            var delete = "DELETE TOP 10 PERCENT" + "FROM target_table;"; // Noncompliant
            var update = "UPDATE dbo.Table2" +
                "SET dbo.Table2.ColB = dbo.Table2.ColB + dbo.Table1.ColB" + // Noncompliant
                "FROM dbo.Table2"; // Noncompliant
            var insert = "INSERT INTO CUSTOMERS (ID,NAME,AGE,ADDRESS,SALARY)" + // FN - we only consider alphanumeric characters
                "VALUES(1, 'Ramesh', 32, 'Ahmedabad', 2000.00);";
            var updateText = "UPDATETEXT @TableName.@ColumnName" +
                "@ptrval @insert_offset"; // Noncompliant
            var merge = "MERGE livesIn" +
                "USING((SELECT @PersonId, @CityId, @StreetAddress) AS T(PersonId, CityId, StreetAddress)" + // Noncompliant
                "    JOIN Person ON T.PersonId = Person.ID" +
                "    JOIN City ON T.CityId = City.ID)" +
                "  ON MATCH(Person-(livesIn)->City)" +
                "WHEN MATCHED THEN" + // FN - we only consider alphanumeric characters
                "  UPDATE SET StreetAddress = @StreetAddress" +
                "WHEN NOT MATCHED THEN" + // Noncompliant
                "  INSERT($from_id, $to_id, StreetAddress)" +
                "  VALUES(Person.$node_id, City.$node_id, @StreetAddress);";
            var writeText = "WRITETEXT pub_info.pr_info" + "ptrval 'text'"; // Noncompliant
            var writeTextWithAt = "WRITETEXT pub_info.pr_info" + "@ptrval 'text'"; // Noncompliant
            var readText = "READTEXT" + "pub_info.pr_info @ptrval 1 25;"; // Noncompliant
        }

        public string Property
        {
            get
            {
                return "SELECT something" + "FROM table"; // Noncompliant
            }
            set
            {
                value = "SELECT x" + "FROM table"; // Noncompliant
            }
        }

        private string field = "SELECT *" +
            "FROM TABLE" +  // Noncompliant
            "WHERE X = 1;"; // Noncompliant

        private string caseInsensitive = "select *" +
            "from t" +  // Noncompliant
            "where X = 1;"; // Noncompliant

        private string onlyFirstKeyword = "select something" +
            "because I want to";  // Noncompliant FP
    }

    class CompliantExamples
    {
        private string sql_string = "SELECT * FROM TABLE WHERE X = 1;";
        private string sql_string_spaces_before = "SELECT *" +
            " FROM TABLE" +
            " WHERE X = 1;";
        private string sql_string_spaces_after = "SELECT * " +
            "FROM TABLE " +
            "WHERE X = 1;";
        private string not_sql_string = "I like tomatoes" + "and potatoes";
        private int not_string = 1 + 2 + 3;
        private string combined = "SELECT all" + 1 + "FROM table";

        void ValidCases(string parameter)
        {
            var withParamInside = "SELECT *" +
                parameter +
                "FROM TABLE" +
                parameter +
                "WHERE X = 1;";
            var withColumnNames = "SELECT x.col1, x.col2," +
                "x.col3,x.col4 " +
                "FROM TABLE" +
                parameter +
                "WHERE X = 1;";
            var withVarAtEnd = "SELECT x " +
                "FROM y " +
                "WHERE z =" + parameter;
            var useGoIfDrop = "USE AdventureWorks2012;" + // ok
                "GO" + // FP
                "IF OBJECT_ID('dbo.Table1', 'U') IS NOT NULL" + // FN, we don't support this
                "DROP TABLE dbo.Table1;";
            var semicolon = "SELECT * FROM table;" +
                "WHERE something = x;" +
                "ORDERBY z";
            var comment = "SELECT * FROM table--" +
                "WHERE something = x--" +
                "ORDERBY z";
            var multilineComment = "SELECT * FROM table/*" +
                "WHERE something = x" + // Noncompliant FP
                "ORDERBY z*/"; // Noncompliant FP
            var nonAlphaNumeric = "SELECT * FROM table+" +
                "WHERE something = x^" +
                "ORDERBY z#" +
                "UNION WITH x";
            var comparison = "SELECT * FROM table WHERE x>" +
                "10 AND y >20;";
            var comparison2 = "SELECT * FROM table WHERE x" +
                "> 10 AND y >20;";
            var parantheses = "SELECT (" +
                "x.y,z.z)" + // FN - we ignore parantheses as they can lead to FPs
                "FROM  table;";
        }

        public void InterpolatedStringsAreIgnored(string col1, string col2, string innerQuery)
        {
            var select = $"SELECT e.{col1},e.{col2}" + // FN
                $"FROM DimEmployee AS e" +
                $"ORDER BY LastName;";
            var interpolatedQuery = $"SELECT x" + $"{innerQuery}";
        }
    }
}
