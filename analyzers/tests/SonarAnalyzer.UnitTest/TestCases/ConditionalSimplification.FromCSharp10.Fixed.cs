using System;
using System.Collections.Generic;

Fruit a = null;
var y = a switch
{
    null => 1,
    { Prop.Count: 1 } => 0 // Fixed
};

class Fruit { public List<int> Prop; }
