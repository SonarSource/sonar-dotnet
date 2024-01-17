using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

var list = new List<int>();
list.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
list?.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
list.Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
list?.Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
list.Append(1).Any(x => x > 0);
list.Any();
list.Exists(x => x > 0);

var immutableList = ImmutableList.Create<int>();
immutableList.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList?.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Append(1).Any(x => x > 0);
immutableList.Any();
immutableList.Exists(x => x > 0);

int someInt = 1;
immutableList.Any(x => x == 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => 0 == x); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => x == someInt); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => someInt == x); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => x.Equals(0)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => 0.Equals(x)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
immutableList.Any(x => x.Equals(x + 1)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
