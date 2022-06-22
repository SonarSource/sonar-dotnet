using System;
using System.Data;
using System.Globalization;

DataTable y, d, f, j, l;
int b, h;

(var k, l) = (new DataTable(), new DataTable()); // Noncompliant
k.Locale = CultureInfo.InvariantCulture;
