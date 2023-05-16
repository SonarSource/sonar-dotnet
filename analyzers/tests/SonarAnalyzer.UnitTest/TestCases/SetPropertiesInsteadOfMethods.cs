using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

static class Program
{
    static void SortedSet()
    {
        var sortedSet = new SortedSet<int>();

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

        new SortedSet<int> { 42 }.Min(); // Noncompliant
        new SortedSet<int> { 42 }.Max(); // Noncompliant

        sortedSet.Min(x => 42f); // Compliant, predicate used
        sortedSet?.Max(x => x); // Compliant, predicate used
    }

    static void DerivesFromSortedSet()
    {
        var sortedSetDerived = new DerivesFromSetType<int>();

        (true ? sortedSetDerived : sortedSetDerived).Min(); // Noncompliant
        (true ? sortedSetDerived : sortedSetDerived).Max(); // Noncompliant

        (sortedSetDerived ?? sortedSetDerived).Min(); // Noncompliant
        (sortedSetDerived ?? sortedSetDerived).Max(); // Noncompliant

        (sortedSetDerived ?? (true ? sortedSetDerived : sortedSetDerived)).Min(); // Noncompliant
        (sortedSetDerived ?? (true ? sortedSetDerived : sortedSetDerived)).Max(); // Noncompliant

        sortedSetDerived.Fluent().Fluent().Fluent().Fluent().Min(); // Noncompliant
        sortedSetDerived.Fluent().Fluent().Fluent().Fluent()?.Max(); // Noncompliant
        sortedSetDerived.Fluent().Fluent().Fluent()?.Fluent().Min(); // Noncompliant
        sortedSetDerived.Fluent().Fluent().Fluent()?.Fluent()?.Max(); // Noncompliant
        sortedSetDerived.Fluent().Fluent()?.Fluent().Fluent().Min(); // Noncompliant
        sortedSetDerived.Fluent().Fluent()?.Fluent().Fluent()?.Max(); // Noncompliant
        sortedSetDerived.Fluent().Fluent()?.Fluent()?.Fluent().Min(); // Noncompliant
        sortedSetDerived.Fluent().Fluent()?.Fluent()?.Fluent()?.Max(); // Noncompliant
        sortedSetDerived.Fluent()?.Fluent().Fluent().Fluent().Min(); // Noncompliant
        sortedSetDerived.Fluent()?.Fluent().Fluent().Fluent()?.Max(); // Noncompliant
        sortedSetDerived.Fluent()?.Fluent().Fluent()?.Fluent().Min(); // Noncompliant
        sortedSetDerived.Fluent()?.Fluent().Fluent()?.Fluent()?.Max(); // Noncompliant
        sortedSetDerived.Fluent()?.Fluent()?.Fluent().Fluent().Min(); // Noncompliant
        sortedSetDerived.Fluent()?.Fluent()?.Fluent().Fluent()?.Max(); // Noncompliant
        sortedSetDerived.Fluent()?.Fluent()?.Fluent()?.Fluent().Min(); // Noncompliant
        sortedSetDerived.Fluent()?.Fluent()?.Fluent()?.Fluent()?.Max(); // Noncompliant
        //                                                       ^^^

        sortedSetDerived.Min(x => x == 42f); // Compliant, comparer used
        sortedSetDerived?.Max(x => x == 42); // Compliant, comparer used
    }

    static void TrueNegatives()
    {
        var set = new SortedSet<int>();
        _ = set.Min; // Compliant
        _ = set.Max; // Compliant

        T Min<T>() => default(T);
        T Max<T>() => default(T);

        Min<int>(); // Compliant
        Max<int>(); // Compliant

        var doesNotDerive = new DoesNotDeriveFromSetType<int>();
        doesNotDerive.Min(); // Compliant, does not derive from Set type
        doesNotDerive.Max(); // Compliant, does not derive from Set type

        var hidden = new DerivesFromSetTypeButHidesEnumerableMethods<int>();
        hidden.Min(); // Compliant, hides LINQ's Min extension
        hidden.Max(); // Compliant, hides LINQ's Max extension

        dynamic dynamicSet = new SortedSet<int>();
        dynamicSet.Min(); // Compliant, dynamic type
        dynamicSet.Max(); // Compliant, dynamic type
    }
}

class DerivesFromSetType<T> : SortedSet<T>
{
    public DerivesFromSetType<T> Fluent() => this;
}

class DerivesFromSetTypeButHidesEnumerableMethods<T> : SortedSet<T>
{
    public T Min() => default(T);
    public T Max() => default(T);
}

class DoesNotDeriveFromSetType<T> : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}
