using System;

namespace Tests.Diagnostics
{
    public class ForLoopIncrementSignCheck
    {
        public void TestMethod(int x, int y, int z)
        {
            for (int i = x; i < y; i++) { }
            for (int i = x; i > y; i++) { } // Noncompliant {{"i" is incremented and will never reach "stop condition".}}
//                          ^^^^^
            for (int i = x; i >= y; i++) { } // Noncompliant

            for (int i = x; i > y; i--) { }
            for (int i = x; i < y; i--) { } // Noncompliant {{"i" is decremented and will never reach "stop condition".}}
//                          ^^^^^
            for (int i = x; i <= y; i--) { } // Noncompliant

            for (int i = x; y > i; i++) { }
            for (int i = x; y < i; i++) { } // Noncompliant
            for (int i = x; y <= i; i++) { } // Noncompliant

            for (int i = x; y < i; i--) { }
            for (int i = x; y > i; i--) { } // Noncompliant
            for (int i = x; y >= i; i--) { } // Noncompliant

            for (int i = x; x < y; i--) { }
            for (int i = x; x > y; i--) { }

            for (int i = x; i > y; i -= 1) { }
            for (int i = x; i > y; i += 1) { } // Noncompliant

            for (int i = x; i > y; i -= +1) { }
            for (int i = x; i > y; i += -x) { }
            for (int i = x; i > y; i += z) { }

            for (int i = x; i > y; i = i - 1) { }
            for (int i = x; i > y; i = i + 1) { } // Noncompliant

            for (int i = x; i > y; i = i + z) { }
            for (int i = x; i > y; i = z + 1) { }
            for (int i = x; i > y; i = i * 2) { }
            for (int i = x; i > y; i = i - z) { }

            var point = new Point();
            for (int i = x; i > y; point.X = i + 1) { }
            for (int i = x; i + 1 < y; i++) { }
            for (int i = x; i < y;) { }
            for (int i = x; i > y; Update()) { }
            for (int i = x; Condition(); i++) { }
            for (int i = x; ; i++) { }
        }

        private void Update()
        {
        }

        private bool Condition()
        {
            return true;
        }
    }

    public class Point
    {
        public int X { get; set; }
    }
}
