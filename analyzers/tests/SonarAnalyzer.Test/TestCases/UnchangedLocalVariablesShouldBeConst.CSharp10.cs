#nullable enable
using System;

int target = 32; // Noncompliant {{Add the 'const' modifier to 'target'.}}
//  ^^^^^^
int usedTarget = 40; // Compliant
const int alreadyConst = 32;

(int i, usedTarget) = (target, target);
(usedTarget, int k) = (alreadyConst, alreadyConst);

var s1 = $"This is a {nameof(target)}";      // Noncompliant {{Add the 'const' modifier to 's1', and replace 'var' with 'string?'.}}
string s2 = $"This is a {nameof(target)}";   // Noncompliant {{Add the 'const' modifier to 's2'.}}
var s3 = "This is a" + $" {nameof(target)}"; // Noncompliant {{Add the 'const' modifier to 's3', and replace 'var' with 'string?'.}}
var s4 = $@"This is a {nameof(target)}";     // Noncompliant {{Add the 'const' modifier to 's4', and replace 'var' with 'string?'.}}
FormattableString s6 = $"hello";             // Compliant
