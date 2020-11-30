var b = true | false;   // Noncompliant {{Correct this '|' to '||'.}}
b = true & false;       // Noncompliant {{Correct this '&' to '&&'.}}
//       ^

b = true && false;

var i = 1 | 2;
