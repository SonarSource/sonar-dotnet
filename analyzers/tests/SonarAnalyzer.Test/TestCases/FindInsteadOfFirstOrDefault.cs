using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class FindInsteadOfFirstOrDefault
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

    public void UnrelatedType(Dummy dummy, AnotherDummy anotherDummy)
    {
        dummy.FirstOrDefault(); // Compliant
        dummy.FirstOrDefault(x => true); // Compliant

        _ = anotherDummy.FirstOrDefault;

        // Nullable
        dummy?.FirstOrDefault(); // Compliant
        dummy?.FirstOrDefault(x => true); // Compliant

        _ = anotherDummy?.FirstOrDefault;
    }

    public class MyList : List<int>
    {
        public MyList Fluent() => this;
    }

    public class HiddenList : List<int>
    {
        public int? FirstOrDefault(Func<int, bool> predicate) => null;
    }

    public static class HelperClass
    {
        public static List<int> DoWorkReturnGroup() => null;
        public static void DoWorkMethodGroup<T>(Func<Func<T, bool>, T> firstOrDefault) { }

        public static bool FilterMethod(int nb) => true;
    }

    public bool FilterMethod(int nb) => true;

    public void ListBasic(List<int> data)
    {
        data.FirstOrDefault(x => true); // Noncompliant {{"Find" method should be used instead of the "FirstOrDefault" extension method.}}
        //   ^^^^^^^^^^^^^^
        data.Find(x => true); // Compliant

        data.FirstOrDefault(); // Compliant
        data.FirstOrDefault(HelperClass.FilterMethod); // Noncompliant
        //   ^^^^^^^^^^^^^^
        data.FirstOrDefault(FilterMethod); // Noncompliant
        //   ^^^^^^^^^^^^^^
    }

    public void ThroughLinq(List<int> data)
    {
        data.Select(x => x * 2).ToList().FirstOrDefault(x => true); // Noncompliant
        //                               ^^^^^^^^^^^^^^
        data.Select(x => x * 2).ToList().Find(x => true); // Compliant
    }

    public void ThroughFunction()
    {
        HelperClass.DoWorkReturnGroup().FirstOrDefault(x => true); // Noncompliant
        //                              ^^^^^^^^^^^^^^
        HelperClass.DoWorkReturnGroup().Find(x => true); // Compliant
    }

    public void ThroughLambda(Func<List<int>> lambda)
    {
        lambda().FirstOrDefault(x => true); // Noncompliant
        //       ^^^^^^^^^^^^^^
        lambda().Find(x => true); // Compliant
    }

    public void WithinALambda()
    {
        _ = new Func<List<int>, int>(list => list.FirstOrDefault(x => true)); // Noncompliant
        //                                        ^^^^^^^^^^^^^^
    }

    public void AsMethodGroup(List<int> data)
    {
        HelperClass.DoWorkMethodGroup<int>(data.FirstOrDefault); // FN, this raise as a SimpleAccessMemberExpression
    }

    public void Miscellaneous(List<int> data)
    {
        _ = nameof(Enumerable.FirstOrDefault); // Compliant
    }

    public static void FirstOrdefaultNotHidden(MyList data)
    {
        data.FirstOrDefault(x => true); // Noncompliant
        //   ^^^^^^^^^^^^^^
        data.Find(x => true); // Compliant
    }

    public static void FirstOrdefaultHidden(HiddenList data)
    {
        data.FirstOrDefault(x => true); // Compliant
        data.Find(x => true); // Compliant
    }

    public static void Nullable(List<int> data = null)
    {
        data?.FirstOrDefault(x => true); // Noncompliant
        //    ^^^^^^^^^^^^^^
        data?.Find(x => true); // Compliant
    }

    public static void SpecialPattern(List<int> data)
    {
        (true ? data : data).FirstOrDefault(x => true); // Noncompliant
        //                   ^^^^^^^^^^^^^^
        (data ?? data).FirstOrDefault(x => true); // Noncompliant
        //             ^^^^^^^^^^^^^^
        (data ?? (true ? data : data)).FirstOrDefault(x => true); // Noncompliant
        //                             ^^^^^^^^^^^^^^
    }

    public static void FluentNullable(MyList data = null)
    {
        data.Fluent().Fluent().Fluent().Fluent().FirstOrDefault(x => true); // Noncompliant
        //                                       ^^^^^^^^^^^^^^
        data.Fluent().Fluent().Fluent().Fluent()?.FirstOrDefault(x => true); // Noncompliant
        //                                        ^^^^^^^^^^^^^^
        data.Fluent().Fluent().Fluent()?.Fluent().FirstOrDefault(x => true); // Noncompliant
        //                                        ^^^^^^^^^^^^^^
        data.Fluent().Fluent()?.Fluent().Fluent().FirstOrDefault(x => true); // Noncompliant
        //                                        ^^^^^^^^^^^^^^
        data.Fluent()?.Fluent().Fluent().Fluent().FirstOrDefault(x => true); // Noncompliant
        //                                        ^^^^^^^^^^^^^^
        data.Fluent().Fluent().Fluent()?.Fluent()?.FirstOrDefault(x => true); // Noncompliant
        //                                         ^^^^^^^^^^^^^^
        data.Fluent().Fluent()?.Fluent().Fluent()?.FirstOrDefault(x => true); // Noncompliant
        //                                         ^^^^^^^^^^^^^^
        data.Fluent().Fluent()?.Fluent()?.Fluent().FirstOrDefault(x => true); // Noncompliant
        //                                         ^^^^^^^^^^^^^^
        data.Fluent()?.Fluent().Fluent().Fluent()?.FirstOrDefault(x => true); // Noncompliant
        //                                         ^^^^^^^^^^^^^^
        data.Fluent()?.Fluent().Fluent()?.Fluent().FirstOrDefault(x => true); // Noncompliant
        //                                         ^^^^^^^^^^^^^^
        data.Fluent()?.Fluent()?.Fluent().Fluent().FirstOrDefault(x => true); // Noncompliant
        //                                         ^^^^^^^^^^^^^^
        data.Fluent().Fluent()?.Fluent()?.Fluent()?.FirstOrDefault(x => true); // Noncompliant
        //                                          ^^^^^^^^^^^^^^
        data.Fluent()?.Fluent().Fluent()?.Fluent()?.FirstOrDefault(x => true); // Noncompliant
        //                                          ^^^^^^^^^^^^^^
        data.Fluent()?.Fluent()?.Fluent().Fluent()?.FirstOrDefault(x => true); // Noncompliant
        //                                          ^^^^^^^^^^^^^^
        data.Fluent()?.Fluent()?.Fluent()?.Fluent().FirstOrDefault(x => true); // Noncompliant
        //                                          ^^^^^^^^^^^^^^
        data.Fluent()?.Fluent()?.Fluent()?.Fluent()?.FirstOrDefault(x => true); // Noncompliant
        //                                           ^^^^^^^^^^^^^^
    }
}

public class ExpressionTree
{
    public void InExpressionTree()
    {
        Expression<Func<List<int>, int>> firstThree = list => list.FirstOrDefault(el => el == 3); // Compliant (IsInExpressionTree)
    }

    public List<string> Repro_7964(IQueryable<List<string>> values) =>                 // See https://github.com/SonarSource/sonar-dotnet/issues/7964
        values.FirstOrDefault(p => p.FirstOrDefault(x => x.Equals("a")) != null); // Compliant - usage in expression tree
}
