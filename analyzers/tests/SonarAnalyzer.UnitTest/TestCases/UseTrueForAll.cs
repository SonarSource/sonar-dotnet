﻿using System;
using System.Collections.Generic;
using System.Linq;

class ListTestcases
{
    void Playground()
    {
        var list = new List<int>();

        var bad = list.All(x => true); // Noncompliant
        var nullableBad = list?.All(x => true); // Noncompliant

        var good = list.TrueForAll(x => true); // Compliant

        List<int> DoWork() => null;
        Func<List<int>, bool> func = l => l.All(x => true); // Noncompliant

        var methodBad = DoWork().All(x => true); // Noncompliant
        var methodGood = DoWork().TrueForAll(x => true); // Compliant

        var nullableMethodBad = DoWork()?.All(x => true); // Noncompliant
        var nullableMethodGood = DoWork()?.TrueForAll(x => true); // Compliant

        var inlineInitialization = new List<int> { 42 }.All(x => true); // Noncompliant

        var imposter = new ContainsAllMethod<int>();
        imposter.All(x => true); // Compliant

        var badList = new BadList<int>();
        badList.All(x => true); // Compliant

        var goodList = new GoodList<int>();
        goodList.All(x => true); // Noncompliant

        var ternary = (true ? list : goodList).All(x => true); // Noncompliant
        var nullCoalesce = (list ?? goodList).All(x => true); // Noncompliant
        var ternaryNullCoalesce = (list ?? (true ? list : goodList)).All(x => true); // Noncompliant

        _ = goodList.Fluent().Fluent().Fluent().Fluent().All(x => true); //Noncompliant
        _ = goodList.Fluent().Fluent().Fluent().Fluent()?.All(x => true); //Noncompliant
        _ = goodList.Fluent().Fluent().Fluent()?.Fluent().All(x => true); //Noncompliant
        _ = goodList.Fluent().Fluent().Fluent()?.Fluent()?.All(x => true); //Noncompliant
        _ = goodList.Fluent().Fluent()?.Fluent().Fluent().All(x => true); //Noncompliant
        _ = goodList.Fluent().Fluent()?.Fluent().Fluent()?.All(x => true); //Noncompliant
        _ = goodList.Fluent().Fluent()?.Fluent()?.Fluent().All(x => true); //Noncompliant
        _ = goodList.Fluent().Fluent()?.Fluent()?.Fluent()?.All(x => true); //Noncompliant
        _ = goodList.Fluent()?.Fluent().Fluent().Fluent().All(x => true); //Noncompliant
        _ = goodList.Fluent()?.Fluent().Fluent().Fluent()?.All(x => true); //Noncompliant
        _ = goodList.Fluent()?.Fluent().Fluent()?.Fluent().All(x => true); //Noncompliant
        _ = goodList.Fluent()?.Fluent().Fluent()?.Fluent()?.All(x => true); //Noncompliant
        _ = goodList.Fluent()?.Fluent()?.Fluent().Fluent().All(x => true); //Noncompliant
        _ = goodList.Fluent()?.Fluent()?.Fluent().Fluent()?.All(x => true); //Noncompliant
        _ = goodList.Fluent()?.Fluent()?.Fluent()?.Fluent().All(x => true); //Noncompliant
        _ = goodList.Fluent()?.Fluent()?.Fluent()?.Fluent()?.All(x => true); //Noncompliant
    }
    class GoodList<T> : List<T>
    {
        public GoodList<T> Fluent() => this;
    }

    class BadList<T> : List<T>
    {
        public bool All(Func<T, bool> predicate) => true;
    }

    class ContainsAllMethod<T>
    {
        public bool All(Func<T, bool> predicate) => true;
    }
}

class ArrayTestcases
{
    void Playground()
    {
        var array = new int[42];

        var bad = array.All(x => true); // Noncompliant
        var nullableBad = array?.All(x => true); // Noncompliant

        var good = Array.TrueForAll(array, x => true); // Compliant

        int[] DoWork() => null;
        Func<int[], bool> func = l => l.All(x => true); // Noncompliant

        var methodBad = DoWork().All(x => true); // Noncompliant
        var methodGood = Array.TrueForAll(DoWork(), x => true); // Compliant

        var nullableMethodBad = DoWork()?.All(x => true); // Noncompliant
        var inlineInitialization = new int[] { 42 }.All(x => true); // Noncompliant

        _ = array.ToArray().ToArray().ToArray().ToArray().All(x => true); //Noncompliant
        _ = array.ToArray().ToArray().ToArray().ToArray()?.All(x => true); //Noncompliant
        _ = array.ToArray().ToArray().ToArray()?.ToArray().All(x => true); //Noncompliant
        _ = array.ToArray().ToArray().ToArray()?.ToArray()?.All(x => true); //Noncompliant
        _ = array.ToArray().ToArray()?.ToArray().ToArray().All(x => true); //Noncompliant
        _ = array.ToArray().ToArray()?.ToArray().ToArray()?.All(x => true); //Noncompliant
        _ = array.ToArray().ToArray()?.ToArray()?.ToArray().All(x => true); //Noncompliant
        _ = array.ToArray().ToArray()?.ToArray()?.ToArray()?.All(x => true); //Noncompliant
        _ = array.ToArray()?.ToArray().ToArray().ToArray().All(x => true); //Noncompliant
        _ = array.ToArray()?.ToArray().ToArray().ToArray()?.All(x => true); //Noncompliant
        _ = array.ToArray()?.ToArray().ToArray()?.ToArray().All(x => true); //Noncompliant
        _ = array.ToArray()?.ToArray().ToArray()?.ToArray()?.All(x => true); //Noncompliant
        _ = array.ToArray()?.ToArray()?.ToArray().ToArray().All(x => true); //Noncompliant
        _ = array.ToArray()?.ToArray()?.ToArray().ToArray()?.All(x => true); //Noncompliant
        _ = array.ToArray()?.ToArray()?.ToArray()?.ToArray().All(x => true); //Noncompliant
        _ = array.ToArray()?.ToArray()?.ToArray()?.ToArray()?.All(x => true); //Noncompliant
    }
}
