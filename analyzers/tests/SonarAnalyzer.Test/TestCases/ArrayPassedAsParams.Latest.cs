using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

const int a = 1;
int[] array = [2, 3];

var class1 = new MyClass(1, [1, 2, 3]);             // Noncompliant
_ = new MyClass(1, []);                             // Noncompliant

// repro for https://github.com/SonarSource/sonar-dotnet/issues/8510
_ = new MyClass(1, [a, .. array]);                  // Compliant

_ = new MyClass2([1], [1, 2, 3]);                   // Noncompliant
_ = new MyClass2([1, 2, 3], 1);

_ = new MyClass3([1, 2, 3], [4, 5, 6]);             // Compliant: jagged array

_ = new MyClass4(class1, new(1, [1, .. array]));    // Compliant
_ = new MyClass4([class1, new(1, [1, .. array])]);  // Noncompliant, outer collection raises, despite the nested spread operator
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

MyClass5 s = new(1, new int[] { 2, 3 }); // Noncompliant
//                  ^^^^^^^^^^^^^^^^^^
MyClass5 s1 = new(1, 2, 3); // Compliant

A(new int[] { 1, 2 });  // Noncompliant
A([1]);                 // Noncompliant
A(1, 2);                // Compliant
A(2);                   // Compliant
B(new int[] { 1, 2 });  // Noncompliant
B([1]);                 // Noncompliant
B(1, 2);                // Compliant
B(2);                   // Compliant
C(new int[] { 1, 2 });  // Noncompliant
C([1]);                 // Noncompliant
C(1, 2);                // Compliant
C(2);                   // Compliant
D(new int[] { 1, 2 });  // Noncompliant
D([1]);                 // Noncompliant
D(1, 2);                // Compliant
D(2);                   // Compliant
I(MyImmutableArray.Create<int>(new int[] { 1, 2 }));  // Compliant
I(MyImmutableArray.Create<int>([1, 2]));              // Compliant
I([1, 2]);              // Noncompliant
I(2);                   // Compliant

static bool A(params int[] array) => true;
static bool B(params Span<int> span) => true;
static bool C(params ReadOnlySpan<int> span) => true;
static bool D(params IEnumerable<int> enumerable) => true;
static bool E(params IReadOnlyCollection<int> readonlyCollection) => true;
static bool F(params IReadOnlyList<int> readonlyList) => true;
static bool G(params ICollection<int> collection) => true;
static bool H(params IList<int> list) => true;
static bool I(params MyImmutableArray<int> collectionBuilder) => true;

class MyClass(int a, params int[] args);
class MyClass2(int[] a, params int[] args);
class MyClass3(params int[][] args);
class MyClass4(params MyClass[] args);
class MyClass5
{
    public MyClass5(int a, params int[] args) { }
}

static class MyImmutableArray
{
    public static MyImmutableArray<T> Create<T>(ReadOnlySpan<T> items) => [];
}

[CollectionBuilder(typeof(MyImmutableArray), "Create")]
public struct MyImmutableArray<T> : IEnumerable<T>
{
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => default!;
    IEnumerator IEnumerable.GetEnumerator() => default!;
}

// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/6977
public class Repro6977_CollectionExpression
{
    class ParamsAttribute : Attribute
    {
        public ParamsAttribute(params string[] values) { }
        public ParamsAttribute(int a, string b, params string[] values) { }
    }

    internal enum Foo
    {
        [Params(["1", "2" ])] // Noncompliant
        Red,

        [Params("1", "2")]
        Yellow
    }
}

public class ImplicitSpanConversion
{
    public static void Test(string[] myArray)
    {
        DoSomething(new string[] { "s1", "s2" });   // Noncompliant {{Remove this array creation and simply pass the elements.}}
        //          ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        DoSomething(new string[] { "s1" });         // Noncompliant
        DoSomething(new[] { "s1" });                // Noncompliant
        DoSomething(new string[] { });              // Noncompliant
        DoSomething("s1");                          // Compliant
        DoSomething("s1", "s2");                    // Compliant
        DoSomething(myArray);                       // Compliant
        DoSomething(new string[12]);                // Compliant
        DoSomething(["1", "2", "3"]);               // Noncompliant
        DoSomething(["1", "2", "3"].ToArray());     // Compliant
                                                    // Error@-1 [CS9176] There is no target type for the collection expression.
    }

    public static void DoSomething(params Span<string> arr) { }
}
