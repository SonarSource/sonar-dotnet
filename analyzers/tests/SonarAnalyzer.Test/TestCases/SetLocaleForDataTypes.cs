using System;
using System.Data;
using System.Globalization;

namespace Tests.Diagnostics
{
    class Program
    {
        private DataTable myTable = new DataTable { Locale = CultureInfo.InvariantCulture };
        private DataTable myWrongTable = new DataTable(); // Noncompliant {{Set the locale for this 'DataTable'.}}
        //                               ^^^^^^^^^^^^^^^
        private DataTable myWrongTable1 = new DataTable(), myWrongTable2 = new DataTable();
        //                                ^^^^^^^^^^^^^^^                                      {{Set the locale for this 'DataTable'.}}
        //                                                                 ^^^^^^^^^^^^^^^ @-1 {{Set the locale for this 'DataTable'.}}

        void Foo()
        {
            var dataTable = new System.Data.DataTable();
            dataTable.Locale = System.Globalization.CultureInfo.InvariantCulture;

            var dataSet = new System.Data.DataSet();
            dataSet.Locale = System.Globalization.CultureInfo.InvariantCulture;

            DataTable dataTable2;
            dataTable2 = new DataTable { Locale = CultureInfo.InvariantCulture };

            var fooBar = new FooBar(new DataTable()); // FN

            var a = new
            {
                MyTable1 = new DataTable { Locale = CultureInfo.InvariantCulture },
                MyTable2 = new DataTable(), // FN
            };
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
            dataTable2 = new DataTable(); // Compliant FN
        }

        void BadSyntax()
        {
            a = new DataTable();  // Error [CS0103]
        }

        void CornerCases()
        {
            new DataTable { Locale = CultureInfo.CurrentUICulture };
            new DataTable();
        }

        void Bar()
        {
            void M(DataTable table) { }
            M(new DataTable()); // FN

            void Init(DataTable dt) => dt.Locale = new CultureInfo("de-DE");
            var datatable = new DataTable(); // Noncompliant FP
            Init(datatable);
        }

        void MultipleVariableDeclarators()
        {
            DataTable myTable1 = new DataTable { Locale = CultureInfo.CurrentUICulture }, myTable2 = new DataTable();
            //                                                                                       ^^^^^^^^^^^^^^^  Noncompliant
            DataTable myTable3 = new DataTable(), myTable4 = new DataTable { Locale = CultureInfo.CurrentUICulture };
            //                   ^^^^^^^^^^^^^^^                                                                      Noncompliant
            DataTable myTable5 = new DataTable(), myTable6 = new DataTable();
            //                   ^^^^^^^^^^^^^^^                                                                      Noncompliant
            //                                               ^^^^^^^^^^^^^^^                                          @-1
        }
    }

    public class FooBar
    {
        public FooBar(DataTable datatable) { }
    }
}
