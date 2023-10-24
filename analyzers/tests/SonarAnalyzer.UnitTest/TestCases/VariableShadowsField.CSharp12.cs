using System;

class MyParentClass(int parentClassParam)
{
    class MyClass(int a, int b, int c, int d, int e, int f, int g)
    {
        private int a = a; // Compliant
        private int b; // FN
        private int c { get; set; } = c; // Compliant
        private int d { get; set; } // FN

        class MyChildClass(int childClassParam) { }

        MyClass(int a, int b) : this(a, b, 1, 1, 1, 1, 1) { } // Compliant

        void MyMethod()
        {
            int parentClassParam = 42; // Compliant
            int childClassParam = 42; // Compliant
            int ctorParam = 42; // Compliant

            int e = 42; // Noncompliant {{Rename 'e' which hides the primary constructor parameter with the same name.}}
            _ = a is object f; // Noncompliant
            for (int g = 0; g < 10; g++) { } // Noncompliant
            for (int a = 0; a < 10; a++) { } // Noncompliant FP
            var lambda = (int g = 1) => g; // Compliant
        }
    }
}
