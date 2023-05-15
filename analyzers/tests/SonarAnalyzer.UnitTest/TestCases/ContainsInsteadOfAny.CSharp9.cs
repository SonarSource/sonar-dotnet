using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

var list = new List<int>();
list.Any(x => x == 0); // Noncompliant
list?.Any(x => x == 0); // Noncompliant
list.Append(1).Any(x => x == 0);
list.Any();
list.Contains(0);

var immutableList = ImmutableList.Create<int>();
int someInt = 1;
immutableList.Any(x => x == 0); // Compliant
immutableList.Any(x => 0 == x); // Compliant
immutableList.Any(x => x == someInt); // Compliant
immutableList.Any(x => someInt == x); // Compliant
immutableList.Any(x => x.Equals(0)); // Compliant
immutableList.Any(x => 0.Equals(x)); // Compliant
immutableList.Any(x => x.Equals(x + 1)); // Compliant
