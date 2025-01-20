using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

static class Program
{
    static List<int> list = new List<int>();
    static void Main() { }

    static void SimpleCase_OrderBy()
    {
        list.OrderBy(x => x).Where(x => true);
        //                   ^^^^^ {{"Where" should be used before "OrderBy"}}
        //   ^^^^^^^ Secondary@-1 {{This "OrderBy" precedes a "Where" - you should change the order.}}

        list.OrderBy(x => x)?.Where(x => true);
        //                    ^^^^^ {{"Where" should be used before "OrderBy"}}
        //   ^^^^^^^ Secondary@-1

        list?.OrderBy(x => x).Where(x => true);
        //                    ^^^^^ {{"Where" should be used before "OrderBy"}}
        //    ^^^^^^^ Secondary@-1

        list?.OrderBy(x => x)?.Where(x => true);
        //                     ^^^^^ {{"Where" should be used before "OrderBy"}}
        //    ^^^^^^^ Secondary@-1
    }

    static void SimpleCase_OrderByDescending()
    {
        list.OrderByDescending(x => x).Where(x => true);
        //                             ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        //   ^^^^^^^^^^^^^^^^^ Secondary@-1 {{This "OrderByDescending" precedes a "Where" - you should change the order.}}

        list.OrderByDescending(x => x)?.Where(x => true);
        //                              ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        //   ^^^^^^^^^^^^^^^^^ Secondary@-1

        list?.OrderByDescending(x => x).Where(x => true);
        //                              ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        //    ^^^^^^^^^^^^^^^^^ Secondary@-1

        list?.OrderByDescending(x => x)?.Where(x => true);
        //                               ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        //    ^^^^^^^^^^^^^^^^^ Secondary@-1
    }

    static void OrderWhereWhere()
    {
        list.OrderBy(x => x).Where(x => true).Where(x => false);
        //                   ^^^^^ {{"Where" should be used before "OrderBy"}}
        //   ^^^^^^^ Secondary@-1

        list.OrderBy(x => x)?.Where(x => true)?.Where(x => false);
        //                    ^^^^^ {{"Where" should be used before "OrderBy"}}
        //   ^^^^^^^ Secondary@-1

        list?.OrderBy(x => x).Where(x => true).Where(x => false);
        //                    ^^^^^ {{"Where" should be used before "OrderBy"}}
        //    ^^^^^^^ Secondary@-1

        list?.OrderBy(x => x)?.Where(x => true)?.Where(x => false);
        //                     ^^^^^ {{"Where" should be used before "OrderBy"}}
        //    ^^^^^^^ Secondary@-1
    }

    static void OrderWhereOrderWhere()
    {
        list.OrderBy(x => x).Where(x => true)
        //                   ^^^^^ {{"Where" should be used before "OrderBy"}}
        //   ^^^^^^^ Secondary@-1
            .OrderBy(x => x).Where(x => true);
        //                   ^^^^^ {{"Where" should be used before "OrderBy"}}
        //   ^^^^^^^ Secondary@-1

        list?.OrderBy(x => x)?.Where(x => true)?
        //                     ^^^^^ {{"Where" should be used before "OrderBy"}}
        //    ^^^^^^^ Secondary@-1
            .OrderBy(x => x)?.Where(x => true);
        //                    ^^^^^ {{"Where" should be used before "OrderBy"}}
        //   ^^^^^^^ Secondary@-1
    }

    static void CompliantCases()
    {
        list.OrderBy(x => x).Select(x => x).Where(x => true); // Compliant
        var ordered = list.OrderBy(x => x);
        ordered.Where(x => true); // Compliant

        var fake = new Fake<int>();
        fake.OrderBy(x => x).Where(x => true); // Compliant

        var semiFake = new SemiFake<int>();
        // "Where" is the LINQ version, "OrderBy" is custom extension
        semiFake.OrderBy(x => x).Where(x => true); // Compliant
    }

    static void CustomImplementation()
    {
        var mine = new MyEnumerable<int>();

        mine.OrderBy(x => x).Where(x => true);
        //                   ^^^^^ {{"Where" should be used before "OrderBy"}}
        //   ^^^^^^^ Secondary@-1

        mine?.OrderByDescending(x => x)?.Where(x => true);
        //                               ^^^^^ {{"Where" should be used before "OrderByDescending"}}
        //    ^^^^^^^^^^^^^^^^^ Secondary@-1
    }

}

public class MyEnumerable<T> : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}

public class Fake<T> { }

public class SemiFake<T> : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}

static class FakeExtensions
{
    public static Fake<TSource> Where<TSource>(this Fake<TSource> source, Func<TSource, bool> predicate) => source;
    public static Fake<TSource> OrderBy<TSource, TKey>(this Fake<TSource> source, Func<TSource, TKey> keySelector) => source;
    public static SemiFake<TSource> OrderBy<TSource, TKey>(this SemiFake<TSource> source, Func<TSource, TKey> keySelector) => source;
}

