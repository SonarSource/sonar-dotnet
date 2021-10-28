using System;

int target = 32; // Noncompliant {{Add the 'const' modifier to 'target'.}}
//  ^^^^^^
int usedTarget = 40; // Compliant
const int alreadyConst = 32;

(int i, int b) = (target, target);
(usedTarget, i) = (target, target);

