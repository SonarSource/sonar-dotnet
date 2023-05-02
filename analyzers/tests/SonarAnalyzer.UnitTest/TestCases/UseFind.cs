using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;


public class UseFind
{
    public class TestDummy
    {
        public object FirstOrDefault() => null;
        public object FirstOrDefault(Func<int, bool> predicate) => null;
    }

    public void UnrelatedType()
    {
        var dummy = new TestDummy();

        _ = dummy.FirstOrDefault(); // Compliant
        _ = dummy.FirstOrDefault(x => true); // Compliant
    }

    public void ListType()
    {

        var data = new List<int>();

        /*_ = data.FirstOrDefault(x => true); // Noncompliant
//               ^^^^^^^^^^^^^^
        _ = data.Find(x => true); // Compliant

        data.Select(x => x * 2).ToList().FirstOrDefault(x => true); // Noncompliant
//                                       ^^^^^^^^^^^^^^
        data.Select(x => x * 2).ToList().Find(x => true); // Compliant

        List<int> DoWork() => null;

        _ = DoWork().FirstOrDefault(x => true); // Noncompliant
//                   ^^^^^^^^^^^^^^
        _ = DoWork().Find(x => true); // Compliant

        _ = new Func<List<int>, int>(list => list.FirstOrDefault(x => true)); // Noncompliant
//                                                ^^^^^^^^^^^^^^

        var lambda = new Func<List<int>>(() => new List<int>());

        _ = lambda().FirstOrDefault(x => true); // Noncompliant
//                   ^^^^^^^^^^^^^^
        _ = lambda().Find(x => true); // Compliant

        _ = data.FirstOrDefault(x => true) switch // Noncompliant
            {
                < 42 => false,
                _ => true
            };*/

        DoWork2<int>(data.FirstOrDefault); // Compliant FN?
//                        ^^^^^^^^^^^^^^
        void DoWork2<T>(Func<Func<T, bool>, T> firstOrDefault)
        {
            firstOrDefault(x => true);
        }
    }
}
