/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.CBDE
{
    /// <summary>
    /// This class contains tests that generate MLIR code from C# source code, then check that the generated
    /// code is valid. It does not compare the generated code to a reference.
    /// </summary>
    [TestClass]
    public class MlirExportTest
    {
        public TestContext TestContext { get; set; } // Set automatically by MsTest

        [ClassInitialize]
        public static void CheckExecutableExists(TestContext tc)
        {
            MlirTestUtilities.CheckExecutableExists();
        }

        [TestMethod]
        public void SimpleMethod()
        {
            const string code = @"
class C
{
    int Mult(int i, int j)
    {
        return i*j;
    }

    void Empty() {}
    void Nop(int i) { int j = 2*i;}

    //int Cond(int i) { return i%2 == 0 ? i/2 : i*3 +1; }
    int Cond2(int i)
    {
        if (i%2 == 0)
            return i/2;
        else
            return i*3 +1;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void IfThenElse()
        {
            const string code = @"
void UselessCondition(int i) {
    if (i == 0) {
        if (i != 0) {
            int j = 2 * i;
        }
    }
}

int WithReturn(int i) {
    if (i == 0) {
        if (i != 0) {
            int j = 2 * i;
        }
    }
    return i;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void WhileLoops()
        {
            const string code = @"
private int WhileLoop(int i)
{
    while (i<100)
    {
        i = i * i;
    }
    return i;
}
private int WhileLoopBreak(int i)
{
    while (i < 100)
    {
        i = i * i;
        if (i == 49)
        {
            break;
        }
    }
    return i;
}
private int WhileLoopContinue(int i)
{
    while (i < 100)
    {
        if (i == 42)
        {
            continue;
        }
    }
    return i;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void DoWhileLoops()
        {
            const string code = @"
private int WhileLoop(int i)
{
    do
    {
        i = i * i;
    } while (i<100)
    return i;
}
private int WhileLoopBreak(int i)
{
    do
    {
        i = i * i;
        if (i == 49)
        {
            break;
        }
    } while (i < 100)
    return i;
}
private int WhileLoopContinue(int i)
{
    do
    {
        if (i == 42)
        {
            continue;
        }
    } while (i < 100)
    return i;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ForLoops()
        {
            const string code = @"
private int ForLoop(int i)
{
    int total = 0;
    for (int j = 0 ; j<=i ;++j)
    {
        total += j;
    }
    return total;
}
private int ForLoopBreak(int i)
{
    int total = 0;
    for (int j = 0 ; j<=i ;++j)
    {
        total += j;
        if (total > 500)
        {
            break;
        }
    }
    return total;
}
private int ForLoopContinue(int i)
{
    int total = 0;
    for (int j = 0 ; j<=i ;++j)
    {
        if (j % 13 == 0)
        {
            continue;
        }
        total += j;
    }
    return total;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ForEachLoops()
        {
            const string code = @"
int ForEachLoop()
{
    var a = new int [10];
    foreach(var i in a)
    {
        if (i == 0)
        {
            return i;
        }
    }
    return 0;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void WorkWithLong()
        {
            const string code = @"
long withLong()
{
    long l = 10;
    long total = 0;
    for(long i = 0; i < l; ++i)
    {
        total += i;
    }
    if (total != 55)
    {
        return total;
    }
    return total;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void TryCatchFinally()
        {
            const string code = @"
int TryCatch(int i)
{
    int j = 3;
    try
    {
        int k = 2;
    }
    catch (System.InvalidOperationException e)
    {
        return j+2;
    }
    return j;
}

int TryFinally(int i)
{
    int j = 3;
    try
    {
        int k = 2;
    }
    finally
    {
        j++;
    }
    return j;
}

int TryCatchFinally(int i)
{
    int j = 3;
    try
    {
        int k = 2;
    }
    catch (System.InvalidOperationException e)
    {
        return j+2;
    }
    finally
    {
        j++;
    }
    return j;
}

int TryTwoCatches(int i)
{
    int j = 3;
    try
    {
        int k = 2;
    }
    catch (System.InvalidOperationException e)
    {
        return j+2;
    }
    catch (System.Exception e)
    {
        return j+2;
    }
    finally
    {
        j++;
    }
    return j+5;
}

int Throw(int i)
{
    if (i> 42)
    {
        throw new System.Exception();
    }
    return 10;
}

int TryThrow(int i)
{
    ++i;
    try
    {
        if (i> 42)
        {
            throw new System.Exception();
        }
        return 10;
    }
    catch (System.Exception e)
    {
        i+= 12;
    }
    return 20;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void FuncCall()
        {
            const string code = @"

class S {
    public static void stat(){}
}
class A {
public int triple(int i) { return 3*i; }
void log(int i) {}

int f(int i)
{
    var result = triple(triple(i));
    log(result);
    unknownVariable = i;
    unknownFunc(i);
    S.stat();
    UnknownClass.func(i);
    Console.WriteLine(i); // Unknown too, since we are missing library/using
    i = i + unknownVariabe;
    return result;
}

int g(A a, int i)
{
    var result = a.triple(i);
    a.log(result);
    return result;
}
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void Using()
        {
            const string code = @"
using System;

class Resource : IDisposable
{
    public void Dispose() {}
}
class A {
    int f(int i)
    {
        using (var r = new Resource()) {
            var result = i + i;
            return i;
        }
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void Enum()
        {
            const string code = @"
using System;

public enum Color { Red, Green, Blue };
private enum Color { Red, Green, Blue };

class A {
    private Color WorkWithEnum(int i, Color col)
    {
        if (col == Color.Blue && i == (int)col)
        {
            Color c = Color.Blue;
            return c;
        }
        else
        {
            return Color.Red;
        }
    }
    private Color WorkWithUnaccessibleEnum(int i, Color col)
    {
        if (col == Color.Blue && i == (int)col)
        {
            Color c = Color.Blue;
            return c;
        }
        else
        {
            return Color.Red;
        }
    }
    private ConsoleColor WorkWithExternalEnum(int i, ConsoleColor col)
    {
        if (col == ConsoleColor.Blue && i == (int)col)
        {
            Color c = ConsoleColor.Blue;
            return c;
        }
        else
        {
            return ConsoleColor.Red;
        }
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void EmptyBody()
        {
            const string code = @"
public static extern void Extern(int p1);
";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void CommentsWithNewLine()
        {
            const string code = @"
void f()
{
    new A() { i = 3 }; // Noncompliant
//          ^^^^^^^^^
    new A() { i = 4 };
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ReturnNullLitteral()
        {
            const string code = @"
public Type f()
{
    return null;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ReturnParenthesizedNullLitteral()
        {
            const string code = @"
public Type f()
{
    return (null);
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ParenthesizedExpressions()
        {
            const string code = @"
public void f(int i, int j)
{
    var k = ((i) + j);
    if (((k) == 0))
    {
        (i = k);
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void UnknownMethodOnMultipleLines()
        {
            const string code = @"
void f(int i, int j)
{
    g(i,
    j);
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ForWithoutCondition()
        {
            const string code = @"
void f(int i)
{
    for (;;)
    {
        i = i + 1;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ChainingAssignments()
        {
            const string code = @"
void f(int i)
{
    int j = i = 0;
    i = j = 10;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void UseOfReadonlyClassField()
        {
            const string code = @"
public class A
{
    readonly int a = 0;

    public int getA()
    {
        return a;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void UseOfConstantClassField()
        {
            const string code = @"
public class A
{
    const int a = 0;

    public int getA()
    {
        return a;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void UnnamedFunctionParameter()
        {
            const string code = @"
int Func(int, int, int i)
{
    return 2*i;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void SimpleLambdaExpression()
        {
            const string code = @"
public System.Linq.Expressions.Expression<Func<int, int>> F()
{
    return x => 2*x;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ParenthesizedLambdaExpression()
        {
            const string code = @"
Action f()
{
    return () => Y();
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void SwitchStatement()
        {
            const string code = @"
int f(int i)
{
    switch (i)
    {
        case 1:
            return 0;
        default:
            return 1;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void NonASCIIEncodings()
        {
            const string code = @"
public void 你好() { }

public void Łódź() { }

public void AsciiThen你好ThenAsciiAgain() { }

public int Łódźअनुلمرadım(int 你好) { return 2 * 你好; }";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void Arglist()
        {
            const string code = @"
public void f(__arglist)
{
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ParamsKeyword()
        {
            const string code = @"
public void f(params int[] args)
        {
        }";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void CheckedUnchecked()
        {
            const string code = @"
public int CheckedStmt(int i)
{
    checked
    {
        var j = i * 2;
        return j;
    }
}
public int CheckedExpr(int i)
{
    return checked(i * 2);
}
public int UncheckedStmt(int i)
{
    unchecked
    {
        var j = i * 2;
        return j;
    }
}
public int UncheckedExpr(int i)
{
    return unchecked(i * 2);
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void Fixed()
        {
            const string code = @"
class Point
{
    public int x;
    public int y;
}

unsafe private static void ModifyFixedStorage()
{
    Point pt = new Point();
    fixed (int* p = &pt.x)
    {
        *p = 1;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void Namespace()
        {
            const string code = @"
namespace Tests
{
    public static void f()
    {
        Tests.g();
    }
}";
            ValidateCodeGeneration(code);
        }
        [TestMethod]
        public void Overloads()
        {
            const string code = @"
namespace N
{
    class A {
        int F(int i) {return 2*i;}
    }
}

class A {
    int F() {return 0;}
    int F(int i) {return 2*i;}
    int F(int i, int j) {return i*j;}
    double F(double d) {return 2*d;}
}

class B {
    int F(int i) {return 2*i;}
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void NestedFunction()
        {
            const string code = @"
void f()
{
    var s = g();
    int g()
    {
        return 0;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void FieldAssignment()
        {
            const string code = @"
class Point
{
    public int x;
    public int y;
}

void f(Point p)
{
    p.x = 2;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void FieldOfThisAssignment()
        {
            const string code = @"
class Point
{
    public int x;
    public int y;

    void f(int a)
    {
        this.x = a;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ArrayAssignment()
        {
            const string code = @"
void f()
{
    int[] array = new int[5];
    array[0] = 2;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void CastOnReturn()
        {
            const string code = @"
int f(char c)
{
    return c;
}

short g(char c)
{
    return c;
}

char h(bool c)
{
    return c;
    }";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void IgnoreParenthesesInAssignment()
        {
            const string code = @"
void f(int a, int b)
{
    a = (b = 2);
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void AssignmentInComparison()
        {
            const string code = @"
int f(int a, int b)
{
    if ((a = 3) < (b = 2))
    {
        return 0;
    }
    return 1;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void AssignmentInBinaryOperator()
        {
            const string code = @"
void f(int a, int b)
{
    a = (b = 2) + 1;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void Goto()
        {
            const string code = @"
void f()
{
    goto Label;
Label:
    return;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void UnknownConstant()
        {
            const string code = @"
class A
{
        private const object NullConst = null;

        void f()
        {
            NullConst.ToString();
        }";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void Property()
        {
            const string code = @"
class A {
    public void DoSomething1() { }
    public bool someCondition1 { get; set; }

    public void Test_SingleLineBlocks()
    {
        if (someCondition1)
        {
            someCondition1 = false;
            DoSomething1(); // Compliant, ignore single line blocks
        }
        else
        {
            DoSomething1();
        }
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void AsyncFunction()
        {
            const string code = @"
class A {
    async System.Threading.Tasks.Task<int> FuncAsync()
    {
        return 3;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void MethodGroup()
        {
            const string code = @"
public static Func<string, bool> CreateFilter()
{
    return string.IsNullOrEmpty;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void MemberFieldAccess()
        {
            const string code = @"
class M
{
    public int[] Parameters => new int[12];
}
class A
{
    private static bool f(M method, M disposableDispose)
    {
        if (method.Parameters.Length != disposableDispose.Parameters.Length)
        {
            return false;
        }
        return true;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void BoolProperty()
        {
            const string code = @"
class A
{
    public bool Toto => true;
}

class B
{
    static bool f()
    {
        A a = new A();
        return a.Toto;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void MethodsCallsOnMultipleLines()
        {
            const string code = @"
class A
{
    public const string myString = ""Blabla"";

    private string f()
    {
        var result = myString
            .Replace('a', 'b')
            .Replace('d', 'e');
        return result;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void AnonymousMethodExpression()
        {
            const string code = @"
class A
{
    public delegate object anonymousMethod(params object[] args);

    public static anonymousMethod GetAnonymousMethod()
    {
        return delegate (object[] args) { return 0; };
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void AnonymousObjectCreation()
        {
            const string code = @"
public int f()
{
    var v = new { a = 108, m = ""Hello"" };
    return v.a;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void AssignmentInReturn()
        {
            const string code = @"
class A
{
    protected int a;

    public int f()
    {
        return a = 1;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void BooleanConstant()
        {
            const string code = @"
public class A
{
    internal const bool myBool = true;

    void f()
    {
        var b = myBool;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void PropertyFromAnotherAssembly()
        {
            const string code = @"

using System;
using System.Collections.ObjectModel;

public class A<T> : Collection<T>
{
    public void RemoveType(Type type)
    {
        for (var i = Count - 1; i >= 0; i--)
        {
            var formatter = this[i];
            if (formatter.GetType() == type)
            {
                RemoveAt(i);
            }
        }
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void InfiniteLoop()
        {
            const string code = @"
int InfiniteLoop(int i) {
    while (true) {
        if (++i == 42) {
            return i;
        }
    }
    // No return here
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void IfDynamic()
        {
            const string code = @"
internal class A
{
    public dynamic HasValue => true;
}

internal class B
{
    public int Func() {
        var a = new A();
        if(a.HasValue)
        {
            return 2;
        }
        return 3;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ModifyFieldParameterAndLocalVariable()
        {
            const string code = @"
class A
{
    private int p;

    public void f(int dim)
    {
        var b = true;
        b = false;
        dim = 2;
        p = 3;
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void BinaryOperatorAssignment()
        {
            const string code = @"
public void f(int i)
{
    i -= 1;
    i += 2;
    i *= 3;
    i /= 4;
    i %= 5;
    i >>= 6;
    i <<= 7;
    i &= 8;
    i |= 9;
    i ^= 10;
}

public void g(int[] array1, long[] array2)
{
    array1[0] += 2;
    array2[0] += 3;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void NoIdentifier()
        {
            const string code = @"
public class A
{
    public int count;
}

public int f()
{
    var a = new A();
    if (--a.count > 0)
    {
        return 1;
    }
    if ((a.count += 1) > 0)
    {
        return 1;
    }
    return 0;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void IncompatibleAssignmentTypes()
        {
            const string code = @"
public int f(int i)
{
    byte b = (byte)i;
    i = b;

    i = b + 0;
    if (b > 0)
    {
        return i;
    }

    return b;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void UnaryNeg()
        {
            const string code = @"
public bool neg()
{
    int i = -12;
    int j = -i;
    if (i + j == 0) {
        return true;
    }
    return false;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void UnaryPlus()
        {
            const string code = @"
public bool plus()
{
    int i = +12;
    int j = +i;
    if (i - j == 0) {
        return true;
    }
    return false;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void MixingUintAndNeg()
        {
            const string code = @"
void f() {
  int j = -10u;
}

void g() {
  int someInt = -2147483648;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void UnsafeStatement()
        {
            const string code = @"
int f() {
  int i = 0;
  unsafe
  {
    i = 1;
  }
  int j += i;
  return j;
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void UnsafeClass()
        {
            const string code = @"
unsafe class C {
  int f()
  {
    int j = 1;
    return j+1;
  }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void BoolAssignmentInsideIfCondition()
        {
            const string code = @"
public class Derived {

protected bool j;

  void f()
  {
    bool k = true;
    j = 0;
    if(j = k)
    {
      k=1;
    }
    else
    {
      k=2;
    }
  }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void TargetTypedNew()
        {
            const string code = @"
using System;
using System.Collections.Generic;
using System.Text;

public class TargetTypedNew
{
    private List<int> list;

    public void Method()
    {
        list = new();
        StringBuilder sb = new();
        StringBuilder sb2 = new(null);
        StringBuilder sb3 = new(length: 4, capacity: 3, startIndex: 1, value: ""fooBar"");
        Console.WriteLine(sb.ToString() + sb2.ToString() + sb3.ToString());
        }
    }
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void LambdaDiscardParameters()
        {
            const string code = @"
using System;
using System.Collections.Generic;
using System.Linq;

public class LambdaDiscardParameters
{
    public void Method()
    {
        var items = Enumerable.Range(0, 2).SelectMany(_ => Enumerable.Range(0, 1).SelectMany(_ => Enumerable.Range(0, 1))).ToList(); // i don't need parameters in nested
        items.ForEach(_ => Console.WriteLine($""Discard test { _}""));

        LinqQuery(items);

        _ = Bar(_ => { return true; });

        Func<int, string, int> explicitTypes = (int _, string _) => 1;

        LocalFunction(1, 1);

        void LocalFunction(int _, int _2)
        { }
    }

    public Func<int, Func<int, bool>> Nested = _ => _ => true;

    private bool Bar(Func<bool, bool> func)
    {
        return func(true);
    }

    private IEnumerable<int> LinqQuery(List<int> list) =>
        from _ in NoOp()
        from k in list
        select k;

    private IEnumerable<int> NoOp() => new[] { 1 };
}";
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void NativeInts()
        {
            const string code = @"
public class NativeInts
{
    public void Method()
    {
        nint i = -1;
        nuint i2 = 42;
    }
}";
            ValidateCodeGeneration(code);
        }

        private void ValidateCodeGeneration(string code) =>
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
    }
}
