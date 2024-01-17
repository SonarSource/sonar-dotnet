using System.Collections.Immutable;
using System.Linq;
using System;

public class FindInsteadOfFirstOrDefault
{
    public static class ImmutableHelperClass
    {
        public static ImmutableList<int> DoWorkReturnGroup() => null;
        public static void DoWorkMethodGroup<T>(Func<Func<T, bool>, T> firstOrDefault) { }
    }

    public void ListBasic(ImmutableList<int> data)
    {
        data.FirstOrDefault(x => true); // Noncompliant {{"Find" method should be used instead of the "FirstOrDefault" extension method.}}
        //   ^^^^^^^^^^^^^^
        data.Find(x => true); // Compliant

        data.FirstOrDefault(); // Compliant
        data.FirstOrDefault(default(int)); // Compliant
        data.FirstOrDefault(x => false, default(int)); // Compliant
    }

    public void ThroughLinq(ImmutableList<int> data)
    {
        data.Select(x => x * 2).ToList().FirstOrDefault(x => true); // Noncompliant
        //                               ^^^^^^^^^^^^^^
        data.Select(x => x * 2).ToList().Find(x => true); // Compliant
    }

    public void ThroughFunction()
    {
        ImmutableHelperClass.DoWorkReturnGroup().FirstOrDefault(x => true); // Noncompliant
        //                                       ^^^^^^^^^^^^^^
        ImmutableHelperClass.DoWorkReturnGroup().Find(x => true); // Compliant
    }

    public void ThroughLambda(Func<ImmutableList<int>> lambda)
    {
        lambda().FirstOrDefault(x => true); // Noncompliant
        //       ^^^^^^^^^^^^^^
        lambda().Find(x => true); // Compliant
    }

    public void WithinALambda()
    {
        _ = new Func<ImmutableList<int>, int>(list => list.FirstOrDefault(x => true)); // Noncompliant
        //                                                 ^^^^^^^^^^^^^^
    }

    public void AsMethodGroup(ImmutableList<int> data)
    {
        ImmutableHelperClass.DoWorkMethodGroup<int>(data.FirstOrDefault); // FN, this raise as a SimpleAccessMemberExpression
    }

    public void Miscellaneous(ImmutableList<int> data)
    {
        _ = nameof(Enumerable.FirstOrDefault); // Compliant
    }

    public static void Nullable(ImmutableList<int> data = null)
    {
        data?.FirstOrDefault(x => true); // Noncompliant
        //    ^^^^^^^^^^^^^^
        data?.Find(x => true); // Compliant
    }

    public static void SpecialPattern(ImmutableList<int> list1, ImmutableList<int> list2, ImmutableList<int> list3)
    {
        (true ? list1 : list2).FirstOrDefault(x => true); // Noncompliant
        //                     ^^^^^^^^^^^^^^
        (list1 ?? list2).FirstOrDefault(x => true); // Noncompliant
        //               ^^^^^^^^^^^^^^
        (list1 ?? (true ? list2 : list3)).FirstOrDefault(x => true); // Noncompliant
        //                                ^^^^^^^^^^^^^^
    }
}
