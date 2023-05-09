using System;
using System.Collections.Generic;
using System.Linq;

var list = new List<int>();

list.OrderBy(x => x).Where(x => true); // Noncompliant
list.OrderBy(x => x)?.Where(x => true); // Noncompliant
list?.OrderBy(x => x).Where(x => true); // Noncompliant
list?.OrderBy(x => x)?.Where(x => true); // Noncompliant

