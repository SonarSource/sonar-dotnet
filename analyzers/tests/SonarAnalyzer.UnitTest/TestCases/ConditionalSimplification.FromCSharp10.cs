using System;
using System.Collections.Generic;

Fruit a = null;
var y = a switch
{
    null => 1,
    not not {Prop.Count: 1 } => 0 // Noncompliant {{Simplify negation here.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^
};

class Fruit { public List<int> Prop; }
