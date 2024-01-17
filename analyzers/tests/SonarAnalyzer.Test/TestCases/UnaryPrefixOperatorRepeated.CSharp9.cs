nint i = 1;
nint k = ~~i; // Noncompliant; same as i
//       ^^
nint m = + +i; // Compliant
nint n = - -i; // Compliant, we care only about !

bool b = false;
bool c = !!!b; // Noncompliant

bool d = !!!b; // Noncompliant {{Use the '!' operator just once or not at all.}}

nuint j = +1;
j = +(+j); // Compliant, not a typo
bool e = !b;
