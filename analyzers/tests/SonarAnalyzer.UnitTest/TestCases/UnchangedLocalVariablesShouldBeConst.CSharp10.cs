#nullable enable
using System;

int target = 32; // Noncompliant {{Add the 'const' modifier to 'target'.}}
//  ^^^^^^
int usedTarget = 40; // Compliant
const int alreadyConst = 32;

(int i, usedTarget) = (target, target);
(usedTarget, int k) = (alreadyConst, alreadyConst);

var s = $"This is { nameof(target) }"; // Noncompliant {{Add the 'const' modifier to 's', and replace 'var' with 'string?'.}}
