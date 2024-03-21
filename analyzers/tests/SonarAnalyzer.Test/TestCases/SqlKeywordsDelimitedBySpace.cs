using System;
using System.Data.SqlClient;
using Linq = System.Linq;

namespace Tests.Diagnostics
{
    // we don't look for objects from the SqlClient namespace, we just look at the usings

    class NonCompliantExamples
    {
        public void VariousSqlKeywords()
        {
            var alterTable = "ALTER TABLE InsurancePolicy" +
                "ADD PERIOD FOR SYSTEM_TIME(SysStartTime, SysEndTime)," + // Noncompliant
                "SysStartTime datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL" + // ok, we ignore comma
                "    DEFAULT SYSUTCDATETIME()," +
                "SysEndTime datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL" +
                "    DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.99999999');";
            var bulkInsert = "BULK INSERT AdventureWorks2012.Sales.SalesOrderDetail" +
               "FROM 'neworders.txt';"; // Noncompliant {{Add a space before 'FROM'.}}
//             ^^^^^^^^^^^^^^^^^^^^^^^
            var create = "CREATE TABLE #t (x" + "INT PRIMARY KEY);"; // Noncompliant
            var select = "SELECT e.*, f" +
                "FROM DimEmployee AS e" + // Noncompliant
                "ORDER BY LastName;"; // Noncompliant
            var delete = "DELETE TOP 10 PERCENT" + "FROM target_table;"; // Noncompliant
            var drop = "DROP TABLE" + "AdventureWorks2012.dbo.SalesPerson2 ;"; // Noncompliant
            var exec = "EXEC ProcWithParameters" + "@name = N'%arm%', @color = N'Black';"; // Noncompliant
            var execute = "EXECUTE ProcWithParameters" + "@name = N'%arm%', @color = N'Black';"; // Noncompliant
            var update = "UPDATE dbo.Table2" +
                "SET dbo.Table2.ColB = dbo.Table2.ColB + dbo.Table1.ColB" + // Noncompliant
                "FROM dbo.Table2"; // Noncompliant
            var grant = "GRANT EXECUTE ON TestProc" + "TO TesterRole WITH GRANT OPTION;"; // Noncompliant
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
            var truncate = "TRUNCATE" + "TABLE HumanResources.JobCandidate;"; // Noncompliant

            var a = "TRUNCATE" + " " + "TABLE HumanResources.JobCandidate;";

            var b1 = "TRUNCATE" + @" " + "TABLE HumanResources.JobCandidate;";
            var b2 = @"TRUNCATE" + @"TABLE HumanResources.JobCandidate;"; // Noncompliant

            var c1 = "TRUNCATE" + $" " + "TABLE HumanResources.JobCandidate;";
            var c2 = $"TRUNCATE" + $"TABLE HumanResources.JobCandidate;"; // Noncompliant

            var d1 = "TRUNCATE" + $@" " + "TABLE HumanResources.JobCandidate;";
            var d2 = $@"TRUNCATE " + $@"TABLE HumanResources.JobCandidate;";
            var d3 = $@"TRUNCATE" + $@"TABLE HumanResources.JobCandidate;"; // Noncompliant

            var e1 = "TRUNCATE" + @$" " + "TABLE HumanResources.JobCandidate;";
            var e2 = @$"TRUNCATE" + @$" TABLE HumanResources.JobCandidate;";
            var e3 = @$"TRUNCATE" + @$"TABLE HumanResources.JobCandidate;"; // Noncompliant

            var f1 = "UPDATE [some_table]" + "SET [some_column] = '42'"; // Noncompliant
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

        private string field = "SELECT col,col2" +
            "FROM TABLE" +  // Noncompliant
            "WHERE X = 1;"; // Noncompliant

        private string caseInsensitive = "select col3" +
            "from t" +  // Noncompliant
            "where X = 1;"; // Noncompliant

        private string onlyFirstKeyword = "select something" +
            "because I want to";  // Noncompliant FP

        private string insideParens =
            ("SELECT x" + "FROM y") + // Noncompliant
            "JOIN" +
            ("SELECT table" + "WHERE x=1"); // Noncompliant
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
        private string empty = string.Empty;
        private string empty2 = "" + "" + "  " + "\\t" + " ";
        private string combined = "SELECT all" + 1 + "FROM table"; // FN
        private string combined2 = "SELECT all" + "" + "FROM table" + "";
        private string notAdd = "SELECT" + 2 * 3 + "FROM table"; // FN
        private string shortWords = "a" + "b" + "c";

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
                "WHERE something = x" +
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
            var parantheses2 = "SELECT " + ("all") + "FROM table"; // Noncompliant {{Add a space before 'FROM'.}}
            //                                       ^^^^^^^^^^^^
        }

        public void InterpolatedAndRawStringsAreIgnored(string col1, string col2, string innerQuery)
        {
            var select1 = $"SELECT e.{col1},e.{col2}" +
                $"FROM DimEmployee AS e" + // Noncompliant {{Add a space before 'FROM'.}}
                $"ORDER BY LastName;"; // Noncompliant {{Add a space before 'ORDER'.}}
            var select2 = $"SELECT e.{col1},e.{col2}" +
                " FROM DimEmployee AS e" +
                "ORDER BY LastName;"; // Noncompliant {{Add a space before 'ORDER'.}}
            var interpolatedQuery1 = $"SELECT x" + $"{innerQuery}"; // Noncompliant {{Add a space before '{innerQuery}'.}}
            var interpolatedQuery2 = "SELECT x" + $"{innerQuery}"; // Noncompliant
            var interpolatedQuery3 = "SELECT x" + $"{ innerQuery }"; // Noncompliant
            var interpolatedQuery4 = "SELECT x " + $"{innerQuery}";
            var interpolatedQuery5 = "SELECT x" + $" {innerQuery}";
            var alterTable = @"ALTER TABLE InsurancePolicy
ADD PERIOD FOR SYSTEM_TIME(SysStartTime, SysEndTime),
SysStartTime datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL
    DEFAULT SYSUTCDATETIME(),
SysEndTime datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL
    DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.99999999');";

        }
    }
}
