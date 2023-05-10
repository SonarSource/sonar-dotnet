using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

var list = new List<int>();
list.Any(x => x > 0); // Noncompliant
list?.Any(x => x > 0); // Noncompliant
list.Append(1).Any(x => x > 0);
list.Any();
list.Exists(x => x > 0);

var immutableList = ImmutableList.Create<int>();
immutableList.Any(x => x > 0); // Noncompliant
immutableList?.Any(x => x > 0); // Noncompliant
immutableList.Append(1).Any(x => x > 0);
immutableList.Any();
immutableList.Exists(x => x > 0);

int someInt = 1;
immutableList.Any(x => x == 0); // Noncompliant (this is not raising S6617)
immutableList.Any(x => 0 == x); // Noncompliant (this is not raising S6617)
immutableList.Any(x => x == someInt); // Noncompliant (this is not raising S6617)
immutableList.Any(x => someInt == x); // Noncompliant (this is not raising S6617)
immutableList.Any(x => x.Equals(0)); // Noncompliant (this is not raising S6617)
immutableList.Any(x => 0.Equals(x)); // Noncompliant (this is not raising S6617)
immutableList.Any(x => x.Equals(x + 1)); // Noncompliant (this is not raising S6617)
