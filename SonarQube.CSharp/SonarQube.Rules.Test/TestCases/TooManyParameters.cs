using System;

namespace Tests.Diagnostics
{
    public class TooManyParameters
    {
        public TooManyParameters(int p1, int p2, int p3) { }
        public TooManyParameters(int p1, int p2, int p3, int p4) { } // Noncompliant

        public void F1(int p1, int p2, int p3) { }

        public void F1(int p1, int p2, int p3, int p4) { } // Noncompliant

        public void F()
        {
            var v1 = new Action<int, int, int>(delegate(int p1, int p2, int p3) { Console.WriteLine(); });
            var v2 = new Action<int, int, int, int>(delegate(int p1, int p2, int p3, int p4) { Console.WriteLine(); }); // Noncompliant
            var v3 = new Action(delegate { });
            var v4 = new Action<int, int, int>((int p1, int p2, int p3) => Console.WriteLine());
            var v5 = new Action<int, int, int, int>((int p1, int p2, int p3, int p4) => Console.WriteLine()); // Noncompliant
            var v6 = new Action<object, object, object>((p1, p2, p3) => Console.WriteLine());
            var v7 = new Action<object, object, object, object>((p1, p2, p3, p4) => Console.WriteLine()); // Noncompliant
        }
    }

    public interface If
    {
        void F1(int p1, int p2, int p3);
        void F2(int p1, int p2, int p3, int p4); // Noncompliant
    }
}
