using System;
using System.Data;
using System.Globalization;

DataTable y, d, f, j, l;
int b, h;

(var x, y) = (new DataTable(), new DataTable()); // Noncompliant [1, 2]

(var a, b) = (new DataTable(), 42); // Noncompliant

(var c, d) = (42, new DataTable()); // Noncompliant

(var e, f) = (new DataTable(), new DataTable());
(e.Locale, f.Locale) = (CultureInfo.InvariantCulture, CultureInfo.InvariantCulture);

(var g, h) = (new DataTable(), 42);
(g.Locale, h) = (CultureInfo.InvariantCulture, 42);

(var i, j) = (42, new DataTable());
(i, j.Locale) = (42, CultureInfo.InvariantCulture);

var dt = new DataTable();
(i, (dt.Locale, h)) = (42, (CultureInfo.InvariantCulture, 0));

(var k, l) = (new DataTable(), new DataTable()); // Noncompliant {{Set the locale for this 'DataTable'.}}
//                             ^^^^^^^^^^^^^^^
k.Locale = CultureInfo.InvariantCulture;

(var x1, var x2) = ((new DataTable(), 42)); // FN
(var x3, var x4) = ((new DataTable()), 42); // Noncompliant
DataTable x5;
((x5), var x6) = ((new DataTable(), 42)); // FN

var (xx, yy) = (new DataTable(), new DataTable()); // Noncompliant [3, 4]
var (xxx, yyy) = (0, (new DataTable(), 2)); // FN
(int xxxx, (DataTable, int) yyyy) = (0, (new DataTable(), 2)); // FN

TupleParameter((new DataTable(), new DataTable())); // FN

void TupleParameter((DataTable, DataTable) dataTableTuple) { }

var (_, (_, (o, _))) = (1, (2, (new DataTable(), 4))); // Noncompliant

DataTable foo, bar;
foo = (bar = new DataTable()); // Noncompliant
var foobar = (bar ??= new DataTable()); // FN

DataTable myTable5, myTable6;
var (myTable1, myTable2) = ((new DataTable { Locale = CultureInfo.InvariantCulture }), new DataTable()); // Noncompliant
//                                                                                     ^^^^^^^^^^^^^^^
(var myTable3, var myTable4) = (new DataTable { Locale = CultureInfo.InvariantCulture }, new DataTable()); // Noncompliant
//                                                                                       ^^^^^^^^^^^^^^^
(myTable5, myTable6) = (new DataTable { Locale = CultureInfo.InvariantCulture }, new DataTable()); // Noncompliant
//                                                                               ^^^^^^^^^^^^^^^
