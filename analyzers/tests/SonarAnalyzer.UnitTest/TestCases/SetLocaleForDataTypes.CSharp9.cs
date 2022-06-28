using System;
using System.Data;
using System.Globalization;

DataTable table1 = new DataTable { Locale = CultureInfo.InvariantCulture };
table1 = new(); // Compliant - FN

var table2 = new DataTable(); // Noncompliant

DataTable x, y;

(x, y) = (new DataTable(), new DataTable()); // Noncompliant [issue1, issue2]

DataTable table3 = new() { Locale = CultureInfo.InvariantCulture };
DataTable table4 = new(); // Noncompliant

DataSet set1 = new(); // Noncompliant

DataSet set2 = new();
set2.Locale = CultureInfo.InvariantCulture;

record Record
{
    void MoreTest()
    {
        var dataTable = new DataTable(); // Noncompliant {{Set the locale for this 'DataTable'.}}

        Action action = static () =>
        {
            DataTable dataTable = new() { Locale = CultureInfo.InvariantCulture };
        };
    }
}
