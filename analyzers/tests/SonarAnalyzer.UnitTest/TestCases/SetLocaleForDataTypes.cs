using System;
using System.Data;
using System.Globalization;

namespace Tests.Diagnostics
{
    class Program
    {
        private DataTable myTable = new DataTable { Locale = CultureInfo.InvariantCulture };
        private DataTable myWrongTable = new DataTable(); // Noncompliant {{Set the locale for this 'DataTable'.}}
//                                       ^^^^^^^^^^^^^^^

        void Foo()
        {
            var dataTable = new System.Data.DataTable();
            dataTable.Locale = System.Globalization.CultureInfo.InvariantCulture;

            var dataSet = new System.Data.DataSet();
            dataSet.Locale = System.Globalization.CultureInfo.InvariantCulture;

            DataTable dataTable2;
            dataTable2 = new DataTable { Locale = CultureInfo.InvariantCulture };
        }

        void Bar(DataColumn column)
        {
            column.Unique = true; // Compliant - not creating the DataTable
        }

        void Bar(DataSet set)
        {
            set.CaseSensitive = true; // Compliant - not creating the DataSet
        }

        DataTable BadCase1()
        {
            var dataTable = new System.Data.DataTable(); // Noncompliant {{Set the locale for this 'DataTable'.}}
            return dataTable;
        }

        DataSet BadCase2()
        {
            var dataSet = new System.Data.DataSet(); // Noncompliant {{Set the locale for this 'DataSet'.}}
            return dataSet;
        }

        DataTable BadCase3()
        {
            DataTable dataTable;
            dataTable = new System.Data.DataTable(); // Noncompliant {{Set the locale for this 'DataTable'.}}
            return dataTable;
        }

        void MoreTest()
        {
            var dataTable1 = new DataTable(); // Noncompliant {{Set the locale for this 'DataTable'.}}
            var dataTable2 = new DataTable();
            dataTable2.Locale = System.Globalization.CultureInfo.InvariantCulture;
        }
    }
}
