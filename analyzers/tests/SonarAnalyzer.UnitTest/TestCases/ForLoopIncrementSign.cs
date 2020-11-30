using System;

namespace Tests.Diagnostics
{
    public class ForLoopIncrementSign
    {
        private int number = 10;

        public void TestMethod(int x, int y, int z)
        {
            for (; ; ) { }
            for (; x < y;) { }
            for (; x < y ; z++) { }
            for (int i=0; i < x; this.number++) { }


            for (int i = x; i < y; i++) { }
            for (int i = x; i > y; i++) { }
//                                 ^^^ Noncompliant {{'i' is incremented and will never reach 'stop condition'.}}
//                          ^^^^^ Secondary@-1
            for (int i = x; i >= y; i++) { } // Noncompliant
// Secondary@-1
            for (int i = x; i < y; ++i) { }
            for (int i = x; i > y; ++i) { } // Noncompliant
// Secondary@-1
            for (int i = x; i > y; --i) { }
            for (int i = x; i < y; --i) { } // Noncompliant
// Secondary@-1
            for (int i = x; i > y; i--) { }
            for (int i = x; i < y; --i) { }
//                                 ^^^ Noncompliant {{'i' is decremented and will never reach 'stop condition'.}}
//                          ^^^^^ Secondary@-1
            for (int i = x; i <= y; i--) { } // Noncompliant
// Secondary@-1

            for (int i = x; y > i; i++) { }
            for (int i = x; y < i; i++) { } // Noncompliant
// Secondary@-1
            for (int i = x; y <= i; i++) { } // Noncompliant
// Secondary@-1

            for (int i = x; y < i; i--) { }
            for (int i = x; y > i; i--) { } // Noncompliant
// Secondary@-1
            for (int i = x; y >= i; i--) { } // Noncompliant
// Secondary@-1

            for (int i = x; x < y; i--) { }
            for (int i = x; x > y; i--) { }

            for (int i = x; i > y; i -= 1) { }
            for (int i = x; i > y; i += 1) { } // Noncompliant
// Secondary@-1

            for (int i = x; i < y; i += 1) { }
            for (int i = x; i < y; i -= 1) { } // Noncompliant
// Secondary@-1

            for (int i = x; i > y; i -= +1) { }
            for (int i = x; i > y; i += -x) { }
            for (int i = x; i > y; i += z) { }

            for (int i = x; i > y; i = i - 1) { }
            for (int i = x; i > y; i = i + 1) { } // Noncompliant
// Secondary@-1

            for (int i = x; i < y; i = i + 1) { }
            for (int i = x; i < y; i = i - 1) { } // Noncompliant
// Secondary@-1

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
            for (int i = x; i < y && x > 2; i++) { }
            for (int i = x; i < y && x > 2; i++, x++) { }
            for (int i = 0, j = 0; i < y; i++) { }
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
