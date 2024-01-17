using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

List<object> list = null;

list.Select(static (element, col) => element is Int32 and > 21).Any(element => element != null); // Compliant
list.Select((object _, int _) => 1).Any(element => element != null); // Compliant

list.Select((element, col) => element as object).Any(element => element != null);  //Noncompliant {{Use 'OfType<object>()' here instead.}}
