using System;
using System.Collections.Generic;
using System.Linq;

var list = new List<int>();

var bad = list.All(x => true); // Noncompliant
var nullableBad = list?.All(x => true); // Noncompliant

var good = list.TrueForAll(x => true); // Compliant

var bad2 = GetList().All(x => true); // Noncompliant
var good2 = GetList().TrueForAll(x => true); // Compliant

Func<List<int>, bool> func = l => l.All(x => true); // Noncompliant

var t = new ImplementsAll();
t.All(); // Compliant

var fl = new FakeList<int>();
fl.All(x => true); // Compliant

var tl = new GoodList<int>();
tl.All(x => true); // Noncompliant

GetListWrapper().listThing.All(x => true); // Noncompliant
GetListWrapper()?.listThing.All(x => true); // Noncompliant
GetListWrapper().listThing?.All(x => true); // Noncompliant
GetListWrapper()?.listThing?.All(x => true); // Noncompliant

List<int> GetList() => null;
ListWrapper? GetListWrapper() => new();

class FakeList<T> : List<T>
{
    public bool All(Predicate<T> match) => true;
}

class GoodList<T> : List<T> { }

class ImplementsAll
{
    public void All() { }
}

public class ListWrapper
{
    public List<int>? listThing = new();
}
