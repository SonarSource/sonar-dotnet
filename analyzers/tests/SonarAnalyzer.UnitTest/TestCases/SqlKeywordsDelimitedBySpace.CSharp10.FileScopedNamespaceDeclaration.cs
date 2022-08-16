using System;
using System.Data.SqlClient;
using Linq = System.Linq;

namespace Tests.Diagnostics;

record Record
{
    public void VariousSqlKeywords()
    {
        var select = "SELECT e.*, f" +
            "FROM DimEmployee AS e" + // Noncompliant
            "ORDER BY LastName;"; // Noncompliant
    }
}

record struct RecordStruct
{
    public RecordStruct() { }

    private string field = "SELECT col,col2" +
        "FROM TABLE" +  // Noncompliant
        "WHERE X = 1;"; // Noncompliant
}
