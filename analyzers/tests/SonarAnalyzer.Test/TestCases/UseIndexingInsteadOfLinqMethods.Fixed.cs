// File header
#pragma warning disable CS0219

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

class Noncompliant
{
    void Simple(List<int> list)
    {
        _ = list[0]; // Fixed
        _ = list.Last(); // Fixed
        _ = list[42]; // Fixed
    }

    void Nullable(List<int> list)
    {
        _ = list?[0]; // Fixed
        _ = list?.Last(); // Fixed
        _ = list?[42]; // Fixed
    }

    void Jagged(int[][] array)
    {
        _ = array[0][0]; // Fixed
        _ = array[0]?[0]; // Fixed
        _ = array[0][2]; // Fixed
        _ = array[42][0]; // Fixed
    }

    void ArrayCreation()
    {
        _ = (new int[42])[0]; // Fixed
        _ = (new[] { 1, 2 })[1]; // Fixed
        _ = (new int[42])?[0]; // Fixed
    }

    void Complex(FluentList fluent)
    {
        _ = ((IList<int>)new List<int> { 42 })[0]; // Fixed
        _ = ((IList<int>)new List<int> { 42 })?.Last(); // Fixed
        _ = fluent.Fluent().Fluent().Fluent().Last(); // Fixed
        _ = fluent.Fluent().Fluent().Fluent()?.Last(); // Fixed
    }

    int[] DoWork()
    {
        return new int[42];
    }

    void MethodCall()
    {
        _ = DoWork()[0]; // Fixed
        _ = DoWork().Last(); // Fixed
        _ = DoWork()[42]; // Fixed

        _ = DoWork()?[0]; // Fixed
        _ = DoWork()?.Last(); // Fixed
        _ = DoWork()?[42]; // Fixed
    }

    static void Expressions()
    {
        Func<List<int>, int> func = l => l[0]; // Fixed

        Expression<Func<List<int>, int>> exprFirst = l => l[0]; // Fixed
        Expression<Func<List<int>, int>> exprLast = l => l.Last();
        Expression<Func<List<int>, int>> exprElementAt = l => l[42]; // Fixed

        Expression<Func<IList<int>, int>> ilistExprFirst = l => l[0]; // Fixed
        Expression<Func<IList<int>, int>> ilistExprLast = l => l.Last();
        Expression<Func<IList<int>, int>> ilistExprElementAt = l => l[42]; // Fixed

        Expression<Func<IReadOnlyList<int>, int>> readonlyExprFirst = l => l[0]; // Fixed
        Expression<Func<IReadOnlyList<int>, int>> readonlyExprLast = l => l.Last();
        Expression<Func<IReadOnlyList<int>, int>> readonlyExprElementAt = l => l[42]; // Fixed

        _ = new List<int> { 42 }[0]; // Fixed
        _ = new List<int> { 42 }.Last(); // Fixed
        _ = new List<int> { 42 }[42]; // Fixed
    }
}

public class FluentList : List<int>
{
    public FluentList Fluent()
    {
        return this;
    }
}
