using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

var sortedSet = ImmutableSortedSet.Create<int>();

_ = sortedSet.Min(); // Noncompliant {{"Min" property of Set type should be used instead of the "Min()" extension method.}}
//            ^^^
_ = sortedSet?.Min(); // Noncompliant {{"Min" property of Set type should be used instead of the "Min()" extension method.}}
//             ^^^
_ = sortedSet.Max(); // Noncompliant {{"Max" property of Set type should be used instead of the "Max()" extension method.}}
//            ^^^
_ = sortedSet?.Max(); // Noncompliant {{"Max" property of Set type should be used instead of the "Max()" extension method.}}
//             ^^^

Func<SortedSet<int>, int> funcMin = x => x.Min(); // Noncompliant
Func<SortedSet<int>, int> funcMax = x => x.Max(); // Noncompliant

SortedSet<int> DoWork() => null;

DoWork().Min(); // Noncompliant
DoWork().Max(); // Noncompliant

DoWork()?.Min(); // Noncompliant
DoWork()?.Max(); // Noncompliant

ImmutableSortedSet.Create<int>().Add(42).Min(); // Noncompliant
ImmutableSortedSet.CreateBuilder<int>().ToImmutable().Max(); // Noncompliant
