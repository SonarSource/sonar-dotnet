using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

static class Program
{
    static void Main() { }

    static void InterfaceCases()
    {
        IList<int> ilist = new List<int>();

        ilist.First(); // Noncompliant
        ilist.Last(); // Noncompliant
        ilist.ElementAt(42); // Noncompliant

        ilist?.First(); // Noncompliant
        ilist?.Last(); // Noncompliant
        ilist?.ElementAt(42); // Noncompliant

        ((IList<int>)new List<int> { 42 }).First(); // Noncompliant
        ((IList<int>)new List<int> { 42 })?.Last(); // Noncompliant
    }

    static void ListCases()
    {
        var list = new List<int>();

        var badFirst = list.First(); // Noncompliant {{Indexing at 0 should be used instead of the "Enumerable" extension method "First"}}
        //                  ^^^^^
        var badLast = list.Last(); // Noncompliant {{Indexing at Count-1 should be used instead of the "Enumerable" extension method "Last"}}
        //                 ^^^^
        var badElementAt = list.ElementAt(42); // Noncompliant {{Indexing should be used instead of the "Enumerable" extension method "ElementAt"}}
        //                      ^^^^^^^^^
        var badFirstNullable = list?.First(); // Noncompliant {{Indexing at 0 should be used instead of the "Enumerable" extension method "First"}}
        //                           ^^^^^
        var badLastNullable = list?.Last(); // Noncompliant {{Indexing at Count-1 should be used instead of the "Enumerable" extension method "Last"}}
        //                          ^^^^
        var badElementAtNullable = list?.ElementAt(42); // Noncompliant {{Indexing should be used instead of the "Enumerable" extension method "ElementAt"}}
        //                               ^^^^^^^^^

        Func<List<int>, int> func = l => l.First(); // Noncompliant

        List<int> DoWork() => null;

        DoWork().First(); // Noncompliant
        DoWork().Last(); // Noncompliant
        DoWork().ElementAt(42); // Noncompliant

        DoWork()?.First(); // Noncompliant
        DoWork()?.Last(); // Noncompliant
        DoWork()?.ElementAt(42); // Noncompliant

        new List<int> { 42 }.First(); // Noncompliant
        new List<int> { 42 }.Last(); // Noncompliant
        new List<int> { 42 }.ElementAt(42); // Noncompliant
    }

    static void IListImplementation()
    {
        var implementsIList = new ImplementsIList<int>();

        (true ? implementsIList : implementsIList).First(); // Noncompliant
        (true ? implementsIList : implementsIList).Last(); // Noncompliant
        (true ? implementsIList : implementsIList).ElementAt(42); // Noncompliant

        (implementsIList ?? implementsIList).First(); // Noncompliant
        (implementsIList ?? implementsIList).Last(); // Noncompliant
        (implementsIList ?? implementsIList).ElementAt(42); // Noncompliant

        (implementsIList ?? (true ? implementsIList : implementsIList)).First(); // Noncompliant
        (implementsIList ?? (true ? implementsIList : implementsIList)).Last(); // Noncompliant
        (implementsIList ?? (true ? implementsIList : implementsIList)).ElementAt(42); // Noncompliant

        implementsIList.Fluent().Fluent().Fluent().Fluent().First(); // Noncompliant
        implementsIList.Fluent().Fluent().Fluent().Fluent()?.Last(); // Noncompliant
        implementsIList.Fluent().Fluent().Fluent()?.Fluent().ElementAt(42); // Noncompliant
        implementsIList.Fluent().Fluent().Fluent()?.Fluent()?.First(); // Noncompliant
        implementsIList.Fluent().Fluent()?.Fluent().Fluent().Last(); // Noncompliant
        implementsIList.Fluent().Fluent()?.Fluent().Fluent()?.ElementAt(42); // Noncompliant
        implementsIList.Fluent().Fluent()?.Fluent()?.Fluent().First(); // Noncompliant
        implementsIList.Fluent().Fluent()?.Fluent()?.Fluent()?.Last(); // Noncompliant
        implementsIList.Fluent()?.Fluent().Fluent().Fluent().ElementAt(42); // Noncompliant
        implementsIList.Fluent()?.Fluent().Fluent().Fluent()?.First(); // Noncompliant
        implementsIList.Fluent()?.Fluent().Fluent()?.Fluent().Last(); // Noncompliant
        implementsIList.Fluent()?.Fluent().Fluent()?.Fluent()?.ElementAt(42); // Noncompliant
        implementsIList.Fluent()?.Fluent()?.Fluent().Fluent().First(); // Noncompliant
        implementsIList.Fluent()?.Fluent()?.Fluent().Fluent()?.Last(); // Noncompliant
        implementsIList.Fluent()?.Fluent()?.Fluent()?.Fluent().ElementAt(42); // Noncompliant
        implementsIList.Fluent()?.Fluent()?.Fluent()?.Fluent()?.First(); // Noncompliant
        //                                                      ^^^^^

        implementsIList.First(x => x == 42); // Compliant, calls with the predicate cannot be replaced with indexes
        implementsIList.Last(x => x == 42); // Compliant, calls with the predicate cannot be replaced with indexes

        void AcceptsFirstOrLast<T>(Func<T> methodWithNoArguments)
        { }
        void AcceptsElementAt<T>(Func<int, T> methodWithOneArgument)
        { }

        AcceptsFirstOrLast(implementsIList.First); //FN this is not an invocation, just a member access
        AcceptsFirstOrLast(implementsIList.Last); //FN this is not an invocation, just a member access
        AcceptsElementAt(implementsIList.ElementAt); //FN this is not an invocation, just a member access
    }

    static void IReadonlyListImplementation()
    {
        var readonlyList = new ImplementsIReadonlyList<int>();
        readonlyList.First(); // Noncompliant
        readonlyList.Last(); // Noncompliant
        readonlyList.ElementAt(42); // Noncompliant

        readonlyList?.First(); // Noncompliant
        readonlyList?.Last(); // Noncompliant
        readonlyList?.ElementAt(42); // Noncompliant

        object obj = null;
        ((IReadOnlyList<int>)obj).First(); // Noncompliant
        ((IReadOnlyList<int>)null).Last(); // Noncompliant
        (null as IReadOnlyList<int>).ElementAt(42); // Noncompliant
    }

    static void TrueNegatives()
    {
        var list = new List<int>();
        _ = list[0]; // Compliant
        _ = list[list.Count - 1]; // Compliant
        _ = list[42]; // Compliant

        T First<T>() => default(T);
        T Last<T>() => default(T);
        T ElementAt<T>(int index) => default(T);

        First<int>(); // Compliant
        Last<int>(); // Compliant
        ElementAt<int>(42); // Compliant

        var fakeList = new FakeList<int>();

        fakeList.First(); // Compliant
        fakeList.Last(); // Compliant
        fakeList.ElementAt(42); // Compliant

        var doesNotImplementIList = new DoesNotImplementIList<int>();

        doesNotImplementIList.First(); // Compliant, does not implement IList
        doesNotImplementIList.Last(); // Compliant, does not implement IList
        doesNotImplementIList.ElementAt(42); // Compliant, does not implement IList
    }
}

class ImplementsIList<T> : IList<T>
{
    public ImplementsIList<T> Fluent() => this;
    public T this[int index] { get => default(T); set => value = default(T); }

    // Everything below is just to satisfy IList<T> implementation, useless otherwise
    public int Count => throw new NotImplementedException();
    public bool IsReadOnly => throw new NotImplementedException();
    public void Add(T item) { }
    public void Clear() { }
    public bool Contains(T item) => false;
    public void CopyTo(T[] array, int arrayIndex) { }
    public int IndexOf(T item) => 42;
    public void Insert(int index, T item) { }
    public bool Remove(T item) => false;
    public void RemoveAt(int index) { }
    public IEnumerator<T> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}

class ImplementsIReadonlyList<T> : IReadOnlyList<T>
{
    public T this[int index] => default(T);
    public int Count => 42;
    public IEnumerator<T> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}

class DoesNotImplementIList<T> : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}

class FakeList<T> { }

static class FakeListExtensions
{
    public static TSource First<TSource>(this FakeList<TSource> source) => default(TSource);
    public static TSource Last<TSource>(this FakeList<TSource> source) => default(TSource);
    public static TSource ElementAt<TSource>(this FakeList<TSource> source, int index) => default(TSource);
}
