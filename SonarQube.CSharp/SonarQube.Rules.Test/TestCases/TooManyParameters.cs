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
            var v1 = delegate(int p1, int p2, int p3) { Console.WriteLine(); };
            var v2 = delegate(int p1, int p2, int p3, int p4) { Console.WriteLine(); }; // Noncompliant
            var v3 = delegate { }
            var v4 = (int p1, int p2, int p3) => Console.WriteLine();
            var v5 = (int p1, int p2, int p3, int p4) => Console.WriteLine(); // Noncompliant
            var v6 = (p1, p2, p3) => Console.WriteLine();
            var v7 = (p1, p2, p3, p4) => Console.WriteLine(); // Noncompliant
        }
    }

    public interface I
    {
        void F1(int p1, int p2, int p3);
        void F2(int p1, int p2, int p3, int p4); // Noncompliant
    }
}
