using System;
using System.Globalization;

public class Program
{
    public void Base(string[] myArray)
    {
        Method(new string[] { "s1", "s2" }); // Noncompliant {{Remove this array creation and simply pass the elements.}}
//             ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Method(new string[] { "s1" }); // Noncompliant
        Method(new[] { "s1" }); // Noncompliant
        Method(new string[] { }); // Noncompliant
        Method("s1");           // Compliant
        Method("s1", "s2");     // Compliant
        Method(myArray);        // Compliant
        Method(new string[12]); // Compliant

        Method2(1, new string[] { "s1", "s2" }); // Noncompliant {{Remove this array creation and simply pass the elements.}}
//                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Method2(1, new string[] { "s1" }); // Noncompliant
        Method2(1, "s1");           // Compliant
        Method2(1, "s1", "s2");     // Compliant
        Method2(1, myArray);        // Compliant
        Method2(1, new string[12]); // Compliant

        Method3(new string[] { "s1", "s2" }); // Compliant
        Method3(new string[] { "s1", "s2" }, "s1"); // Compliant
        Method3(new string[] { "s1", "s2" }, new string[12]); // Compliant
        Method3(new string[] { "s1", "s2" }, new string[] { "s1", "s2" }); // Noncompliant
//                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Method3(null, null);        // Compliant
        Method4(new[] { "s1" });   // Compliant
        Method4(new[] { "s1", "s2" }); // Compliant

        Method3(args: new string[] { "s1", "s2" }, a: new string[12]); // Compliant (if you specifically require the arguments to be passed in this order there is no way of making this compliant, thus we shouldn't raise)
        Method3(args: new string[12], a: new string[] { "s1", "s2" }); // Compliant

        var s = new MyClass(1, new int[] { 2, 3 }); // Noncompliant
//                             ^^^^^^^^^^^^^^^^^^
        var s1 = new MyClass(1, 2, 3); // Compliant
        s1 = new MyClass(args: new int[] { 2, 3 }, a: 1); // Compliant (if you specifically require the arguments to be passed in this order there is no way of making this compliant, thus we shouldn't raise)
        var s2 = new MyOtherClass(args: new int[12], a: new int[] { 2, 3 }); // Compliant

        var s3 = new IndexerClass();
        var indexer1 = s3[new int[] { 1, 2 }]; // FN
        var indexer2 = s3?[new int[] { 1, 2 }]; // FN
        var indexer3 = s3[1, 2]; // Compliant
    }

    public void Method(params string[] args) { }

    public void Method2(int a, params string[] args) { }

    public void Method3(string[] a, params string[] args) { }

    public void Method4(object[] a, params object[] args) { }

    public void Method5(params string[] a, params string[] args) { } // Error [CS0231]
}

public class MyClass
{
    public MyClass(int a, params int[] args) { }
}

public class MyOtherClass
{
    public MyOtherClass(int[] a, params int[] args) { }
}

public class IndexerClass
{
    public int this[params int[] i] => 1;
}

public class Repro6894
{
    //Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/6894

    public void Method(params object[] args) { }
    public void MethodMixed(int i, params object[] args) { }
    public void MethodArray(params Array[] args) { }
    public void MethodJaggedArray(params int[][] args) { }
    public void MethodImplicitArray(params string[] args) { }

    public void CallMethod(dynamic d)
    {
        Method(new String[] { "1", "2" });                                      // Noncompliant, elements in args: ["1", "2"]
                                                                                // The argument given for a parameter array can be a single expression that is implicitly convertible (§10.2) to the parameter array type.
                                                                                // In this case, the parameter array acts precisely like a value parameter.
                                                                                // see: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#14625-parameter-arrays
        Method(new object[] { new int[] { 1, 2 } });                            // FN, elements in args: [System.Int32[]]
        Method(new int[] { 1, 2, 3, });                                         // Compliant, Elements in args: [System.Int32[]]
        Method(new String[] { "1", "2" }, new String[] { "1", "2" });           // Compliant, elements in args: [System.String[], System.String[]]
        Method(new String[] { "1", "2" }, new int[] { 1, 2 });                    // Compliant, elements in args: [System.String[], System.Int32[]]
        MethodMixed(1, new String[] { "1", "2" });                              // Noncompliant
        MethodArray(new String[] { "1", "2" }, new String[] { "1", "2" });      // Compliant, elements in args: [System.String[], System.String[]]
        MethodArray(new int[] { 1, 2 }, new int[] { 1, 2 });                    // Compliant, elements in args: [System.Int32[], System.Int32[]]
        MethodJaggedArray(new int[] { 1, 2 });                                  // Compliant: jagged array [System.Object[]]
        Method(d);                                                              // Compliant
        Method("Hello", 2);                                                     // Compliant
        string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { "", new object() });
        MethodImplicitArray(new[] { "Hello", "Hi" });                           // Noncompliant , Implicit array creation
        Method(new[] { 1, 2, 3, });                                             // Compliant, Elements in args: [System.Int32[]]
        Method(new[,] { { 1, 2 } });                                            // Compliant, Elements in args: [System.Int32[,]]
        Method(new object[] { null });                                          // Compliant
        Method(new object[,] { { null } });                                     // Compliant
        Method(new[,] { { new object() } });                                    // Compliant
        Method(new object[][] { new[] { new object() } });                      // Compliant
    }
}

// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/6893
public class Repro6893
{
    public void Method(int a, params object[] argumentArray) { }

    public void CallMethod()
    {
        Method(a: 1, argumentArray: new int[] { 1, 2 }); // Compliant
    }
}

// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/6977
public class Repro6977
{
    class ParamsAttribute : Attribute
    {
        public ParamsAttribute(params string[] values) { }
        public ParamsAttribute(int a, string b, params string[] values) { }
        public ParamsAttribute() { }
        public string Name { get; set; }
        public object Other { get; set; }
    }

    internal enum Foo
    {
        [Params(new[] { "1", "2" })] // Noncompliant
        Red,

        [Params("1", "2")]
        Yellow,

        [Params(a: 1, b: "hello", values : new[] { "1", "2" })] // Noncompliant
        Blue,

        [Params(a: 1, values: new[] { "1", "2" }, b: "hello")]  // FN
        Green,

        [Params]
        Violet,

        [Params(Name = "hello", Other = new[] { "1", "2" })] // Compliant, this is setting Properties
        Indigo,

        [Params(a: 1, b: "hello", values: new[] { "1", "2" }, Name = "hello")] // Compliant. Before C# 7.2 the 'correct' version does not compile 
        Orange,

        [Params(1,"hello", new[] { "1", "2" }, Name = "hello")] // Compliant FN 
        Grun,

        [Params(1, "hello", "1", "2", Name = "hello")] // Compliant 
        OtherGrun
    }
}
