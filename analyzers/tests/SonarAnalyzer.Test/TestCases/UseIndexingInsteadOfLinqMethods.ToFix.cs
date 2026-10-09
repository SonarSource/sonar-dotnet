using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

class Noncompliant
{
    void Simple(List<int> list)
    {
        _ = list.First(); // Noncompliant
        _ = list.Last(); // Noncompliant
        _ = list.ElementAt(42); // Noncompliant
    }

    void Nullable(List<int> list)
    {
        _ = list?.First(); // Noncompliant
        _ = list?.Last(); // Noncompliant
        _ = list?.ElementAt(42); // Noncompliant
    }

    void Complex(FluentList fluent)
    {
        _ = ((IList<int>)new List<int> { 42 }).First(); // Noncompliant
        _ = ((IList<int>)new List<int> { 42 })?.Last(); // Noncompliant
        _ = fluent.Fluent().Fluent().Fluent().Last(); // Noncompliant
        _ = fluent.Fluent().Fluent().Fluent()?.Last(); // Noncompliant
    }

    int[] DoWork()
    {
        return new int[42];
    }

    void MethodCall()
    {
        _ = DoWork().First(); // Noncompliant
        _ = DoWork().Last(); // Noncompliant
        _ = DoWork().ElementAt(42); // Noncompliant

        _ = DoWork()?.First(); // Noncompliant
        _ = DoWork()?.Last(); // Noncompliant
        _ = DoWork()?.ElementAt(42); // Noncompliant
    }

    static void Expressions()
    {
        Func<List<int>, int> func = l => l.First(); // Noncompliant

        Expression<Func<List<int>, int>> exprFirst = l => l.First(); // Noncompliant
        Expression<Func<List<int>, int>> exprLast = l => l.Last(); // Noncompliant
        Expression<Func<List<int>, int>> exprElementAt = l => l.ElementAt(42); // Noncompliant

        Expression<Func<IList<int>, int>> ilistExprFirst = l => l.First(); // Noncompliant
        Expression<Func<IList<int>, int>> ilistExprLast = l => l.Last(); // Noncompliant
        Expression<Func<IList<int>, int>> ilistExprElementAt = l => l.ElementAt(42); // Noncompliant

        Expression<Func<IReadOnlyList<int>, int>> readonlyExprFirst = l => l.First(); // Noncompliant
        Expression<Func<IReadOnlyList<int>, int>> readonlyExprLast = l => l.Last(); // Noncompliant
        Expression<Func<IReadOnlyList<int>, int>> readonlyExprElementAt = l => l.ElementAt(42); // Noncompliant

        _ = new List<int> { 42 }.First(); // Noncompliant
        _ = new List<int> { 42 }.Last(); // Noncompliant
        _ = new List<int> { 42 }.ElementAt(42); // Noncompliant
    }
}

public class FluentList : List<int>
{
    public FluentList Fluent()
    {
        return this;
    }
}
