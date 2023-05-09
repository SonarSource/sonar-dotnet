using System;
using System.Linq;
using System.Collections.Immutable;

class ImmutableListTestcases
{
    void Playground()
    {
        var immutable = ImmutableList<int>.Empty;

        var bad = immutable.All(x => true); // Noncompliant
        var nullableBad = immutable?.All(x => true); // Noncompliant

        var good = immutable.TrueForAll(x => true); // Compliant

        int[] DoWork() => null;
        Func<int[], bool> func = l => l.All(x => true); // Noncompliant

        var methodBad = DoWork().All(x => true); // Noncompliant
        var methodGood = Array.TrueForAll(DoWork(), x => true); // Compliant

        var nullableMethodBad = DoWork()?.All(x => true); // Noncompliant
        var inlineInitialization = new int[] { 42 }.All(x => true); // Noncompliant

         immutable.Fluent().Fluent().Fluent().Fluent().All(x => true); //Noncompliant
         immutable.Fluent().Fluent().Fluent().Fluent()?.All(x => true); //Noncompliant
         immutable.Fluent().Fluent().Fluent()?.Fluent().All(x => true); //Noncompliant
         immutable.Fluent().Fluent().Fluent()?.Fluent()?.All(x => true); //Noncompliant
         immutable.Fluent().Fluent()?.Fluent().Fluent().All(x => true); //Noncompliant
         immutable.Fluent().Fluent()?.Fluent().Fluent()?.All(x => true); //Noncompliant
         immutable.Fluent().Fluent()?.Fluent()?.Fluent().All(x => true); //Noncompliant
         immutable.Fluent().Fluent()?.Fluent()?.Fluent()?.All(x => true); //Noncompliant
         immutable.Fluent()?.Fluent().Fluent().Fluent().All(x => true); //Noncompliant
         immutable.Fluent()?.Fluent().Fluent().Fluent()?.All(x => true); //Noncompliant
         immutable.Fluent()?.Fluent().Fluent()?.Fluent().All(x => true); //Noncompliant
         immutable.Fluent()?.Fluent().Fluent()?.Fluent()?.All(x => true); //Noncompliant
         immutable.Fluent()?.Fluent()?.Fluent().Fluent().All(x => true); //Noncompliant
         immutable.Fluent()?.Fluent()?.Fluent().Fluent()?.All(x => true); //Noncompliant
         immutable.Fluent()?.Fluent()?.Fluent()?.Fluent().All(x => true); //Noncompliant
         immutable.Fluent()?.Fluent()?.Fluent()?.Fluent()?.All(x => true); //Noncompliant
    }
}

public static class BuilderExtensions
{
    public static ImmutableList<int> Fluent(this ImmutableList<int> list) => list;
}
