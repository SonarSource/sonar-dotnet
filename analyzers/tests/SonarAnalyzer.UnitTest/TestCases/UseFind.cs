using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;


public class UseFind
{
    public class Dummy
    {
        public object FirstOrDefault() => null;
        public object FirstOrDefault(Func<int, bool> predicate) => null;
    }

    public class AnotherDummy
    {
        public object FirstOrDefault => null;
    }

    public class MyList : List<int>
    {
    }

    public class HiddenList : List<int>
    {
        public int? FirstOrDefault(Func<int, bool> predicate) => null;
    }

    public void UnrelatedType(Dummy dummy, AnotherDummy anotherDummy)
    {
        _ = dummy.FirstOrDefault(); // Compliant
        _ = dummy.FirstOrDefault(x => true); // Compliant

        _ = anotherDummy.FirstOrDefault;
    }

    public void ListBasic(List<int> data)
    {
        _ = data.FirstOrDefault(x => true); // Noncompliant
        //       ^^^^^^^^^^^^^^
        _ = data.Find(x => true); // Compliant
    }

    public void ThroughLinq(List<int> data)
    {
        data.Select(x => x * 2).ToList().FirstOrDefault(x => true); // Noncompliant
        //                               ^^^^^^^^^^^^^^
        data.Select(x => x * 2).ToList().Find(x => true); // Compliant
    }

    public void ThroughFunction()
    {
        List<int> DoWork() => null;

        _ = DoWork().FirstOrDefault(x => true); // Noncompliant
        //           ^^^^^^^^^^^^^^
        _ = DoWork().Find(x => true); // Compliant
    }

    public void ThroughLambda()
    {
        List<int> DoWork() => null;

        _ = DoWork().FirstOrDefault(x => true); // Noncompliant
        //           ^^^^^^^^^^^^^^
        _ = DoWork().Find(x => true); // Compliant
    }

    public void WithinALambda(Func<List<int>> lambda)
    {
        _ = lambda().FirstOrDefault(x => true); // Noncompliant
        //           ^^^^^^^^^^^^^^
        _ = lambda().Find(x => true); // Compliant
    }

    public void WithinALambda()
    {
        _ = new Func<List<int>, int>(list => list.FirstOrDefault(x => true)); // Noncompliant
        //                                        ^^^^^^^^^^^^^^
    }

    public void AsMethodGroup(List<int> data)
    {
        void DoWork<T>(Func<Func<T, bool>, T> firstOrDefault) { }

        DoWork<int>(data.FirstOrDefault); // Noncompliant
        //               ^^^^^^^^^^^^^^
    }

    public void Miscellaneous(List<int> data)
    {
        _ = nameof(Enumerable.FirstOrDefault); // Compliant
    }

    public static void FirstOrdefaultNotHidden(MyList data)
    {
        _ = data.FirstOrDefault(x => true); // Noncompliant
        //       ^^^^^^^^^^^^^^
        _ = data.Find(x => true); // Compliant
    }

    public static void FirstOrdefaultHidden(HiddenList data)
    {
        _ = data.FirstOrDefault(x => true); // Compliant
        _ = data.Find(x => true); // Compliant
    }

    public static void Nullable(List<int> data = null, Dummy dummy = null, AnotherDummy anotherDummy = null)
    {
        _ = data?.FirstOrDefault(x => true); // Noncompliant
        //        ^^^^^^^^^^^^^^
        _ = data?.Find(x => true); // Compliant

        _ = dummy?.FirstOrDefault(); // Compliant
        _ = dummy?.FirstOrDefault(x => true); // Compliant

        _ = anotherDummy?.FirstOrDefault;
    }
}
