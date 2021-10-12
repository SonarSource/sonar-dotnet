using System;
using System.Data.SqlClient;
using Linq = System.Linq;

namespace Tests.Diagnostics;

record Record
{
    public void VariousSqlKeywords()
    {
        var select = "SELECT e.*, f" +
            "FROM DimEmployee AS e" + // FN
            "ORDER BY LastName;"; // FN
    }
}

record struct RecordStruct
{
    private string field = "SELECT col,col2" +
        "FROM TABLE" +  // FN
        "WHERE X = 1;"; // FN
}
