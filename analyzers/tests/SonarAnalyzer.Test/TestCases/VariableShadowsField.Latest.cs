using System;

public class CSharp8LocalFunctions
{
    private int field;
    private int Property { get; set; }

    public void ShadowField()
    {
        static void doMore()
        {
            int field = 0; // Noncompliant
        }
    }

    public void ShadowProperty()
    {
        static void doMore()
        {
            int Property = 0; // Noncompliant
        }
    }

    public void MethodWithLocalVar()
    {
        bool isUsed = true;

        void doSomething()
        {
            bool isUsed = true; // Compliant - currently the rule only looks at fields and properties
        }

        static void doMore()
        {
            bool isUsed = true; // Compliant
        }
    }
}

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
    public static void StaticMethod()
    {
        var primaryCtorParam = 42;      // Compliant (primary ctor parameter is not accessible from static method)
    }

    public void NonStaticMethod()
    {
        var primaryCtorParam = 42;      // Noncompliant
    }
}

public partial class VariableShadowsField
{
    public int myField;
    public int myProperty { get; set; }
}

public partial class VariableShadowsFieldPrimaryConstructor(int a) { }

public class SomeClass
{
    private byte[] somefield;

    public void SomeMethod(byte[] byteArray)
    {
        if (byteArray is [1, 2, 3] somefield) // Noncompliant {{Rename 'somefield' which hides the field with the same name.}}
        {
        }
    }
}

public record struct S
{
    private int i = 0;
    private int j = 0;
    private int k = 0;
    private int l = 0;
    private int n = 0;
    private int p = 0;
    private int q = 0;
    private int r = 0;
    private int s = 0;
    private int t = 0;
    private int u = 0;
    private int v = 0;
    private int w = 0;

    public S()
    {
        (var i, var j) = (0, 0);
        //   ^
        //          ^ @-1

        var (k, l) = (0, 0);     // Noncompliant [issue1,issue2]
        (var m, var n) = (0, 0); // Noncompliant m is not declared as field
        //          ^
        var (o, p) = (0, 0);     // Noncompliant
        (var a, var b) = (0, 0); // Compliant
        var (c, d) = (0, 0);     // Compliant
        var (_, _) = (0, 0);     // Compliant

        var (q, (_, r, _), s) = (1, (2, 3, 4), 5);
        //   ^
        //          ^        @-1
        //                 ^ @-2
        foreach ((var t, var u) in new[] { (1, 2) })
        //            ^
        //                   ^ @-1
        {

        }
        foreach (var (v, w) in new[] { (1, 2) })
        //            ^
        //               ^ @-1
        {

        }
    }
}
public partial class PartialProperty
{
    public partial int myPartialProperty { get; set; }
}

public static class Extensions
{
    public class Sample
    {
        public int Value { get; set; }
    }

    private static int field;
    public static int Property => 42;

    extension(Sample s)
    {
        public void ShadowContainerClass()
        {
            int field = 0;      // FN https://sonarsource.atlassian.net/browse/NET-2846
            int Property = 0;   // FN https://sonarsource.atlassian.net/browse/NET-2846
        }

        public void ShadowExtendedClass()
        {
            int value = 0;
        }
    }
}

public class FieldKeyword
{
    private int field;
    public int Property { get { return field; } set { value = field; } }
}

public partial class PartialConstructor
{
    private int field;
    public partial PartialConstructor();
}

public partial class PartialEvent
{
    private int field;
    public partial event EventHandler myPartialEvent;
}
