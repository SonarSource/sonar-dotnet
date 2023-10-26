using System;

class MyParentClass(int parentClassParam)
{
    class MyClass(int a, int b, int c, int d, int e, int f, int g, int h, int i, int j)
    {
        public int a = a; // Compliant
        public int b; // FN

        public int c { get; set; } = c; // Compliant
        public int d { get; set; } // FN

        public int g = a; // FN {{Rename 'g' which hides the primary constructor parameter with the same name.}}

        class MyChildClass(int childClassParam) { }

        MyClass(int ctorParam, int f) : this(ctorParam, f, 1, 1, 1, 1, 1, 1, 1, 1) { } // Compliant

        void MyMethod()
        {
            int parentClassParam = 42; // Compliant
            int childClassParam = 42; // Compliant
            int ctorParam = 42; // Compliant

            int f = 42; // Noncompliant {{Rename 'f' which hides the primary constructor parameter with the same name.}}
            _ = a is object g; // Noncompliant
            for (int h = 0; h < 10; h++) { } // Noncompliant {{Rename 'h' which hides the primary constructor parameter with the same name.}}
            for (int a = 0; a < 10; a++) { } // Noncompliant {{Rename 'a' which hides the field with the same name.}}
            for (int c = 0; c < 10; c++) { } // Noncompliant {{Rename 'c' which hides the property with the same name.}}
            var lambda = (int h = 1) => h; // Compliant
            foreach (var (i, j) in new (int, int)[0]) { } // Noncompliant
                                                          // Noncompliant@-1
        }
    }
}

// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/8260
class Repro_8260(int primaryCtorParam)
{
    private int instanceField = 1;
    private int instanceProperty { get; set; } = 1;
    private static int staticField = 1;
    private static int staticProperty = 1;

    public static void StaticMethod()
    {
        var instanceField = 2; // Noncompliant FP (instance field is not accessible from static method)
        var instanceProperty = 2; // Noncompliant FP (instance property is not accessible from static method)
        var primaryCtorParam = 2; // Noncompliant FP (primary ctor parameter is not accessible from static method)

        var staticField = 2; // Noncompliant
        var staticProperty = 2; // Noncompliant
    }
}
