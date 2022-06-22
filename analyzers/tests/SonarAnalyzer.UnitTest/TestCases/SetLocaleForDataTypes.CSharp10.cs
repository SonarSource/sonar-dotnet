using System;
using System.Data;
using System.Globalization;

DataTable y, d, f, j, l;
int b, h;

(var x, y) = (new DataTable(), new DataTable()); // Noncompliant
// Noncompliant@-1

(var a, b) = (new DataTable(), 42); // Noncompliant

(var c, d) = (42, new DataTable()); // Noncompliant

(var e, f) = (new DataTable(), new DataTable());
(e.Locale, f.Locale) = (CultureInfo.InvariantCulture, CultureInfo.InvariantCulture);

(var g, h) = (new DataTable(), 42);
(g.Locale, h) = (CultureInfo.InvariantCulture, 42);

(var i, j) = (42, new DataTable());
(i, j.Locale) = (42, CultureInfo.InvariantCulture);

(var k, l) = (new DataTable(), new DataTable()); // Noncompliant
k.Locale = CultureInfo.InvariantCulture;

TupleParameter((new DataTable(), new DataTable()));

void TupleParameter((DataTable, DataTable) dataTableTuple) { }
