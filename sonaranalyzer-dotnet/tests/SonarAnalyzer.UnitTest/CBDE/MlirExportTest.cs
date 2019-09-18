extern alias csharp;
using System.IO;
using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.CBDE
{
    [TestClass]
    public class MlirExportTest
    {

        [ClassInitialize]
        public static void checkExecutableExists(TestContext tc)
        {
            MlirTestUtilities.checkExecutableExists();
        }

        public TestContext TestContext { get; set; } // Set automatically by MsTest


        [TestMethod]
        public void SimpleMethod()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void IfThenElse()
        {
            var code = @"
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
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }


        [TestMethod]
        public void WhileLoops()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);

        }

        [TestMethod]
        public void DoWhileLoops()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);

        }


        [TestMethod]
        public void ForLoops()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);

        }
        [TestMethod]
        public void ForEachLoops()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void WorkWithLong()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }
        [TestMethod]
        public void TryCatchFinally()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void FuncCall()
        {
            var code = @"

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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }
        [TestMethod]
        public void Using()
        {
            var code = @"
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
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void Enum()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void EmptyBody()
        {
            var code = @"
public static extern void Extern(int p1);
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void CommentsWithNewLine()
        {
            var code = @"
void f()
{
    new A() { i = 3 }; // Noncompliant
//          ^^^^^^^^^
    new A() { i = 4 };
}

";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void ReturnNullLitteral()
        {
            var code = @"
public Type f()
{
    return null;
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void ReturnParenthesizedNullLitteral()
        {
            var code = @"
public Type f()
{
    return (null);
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void ParenthesizedExpressions()
        {
            var code = @"
public void f(int i, int j)
{
    var k = ((i) + j);
    if (((k) == 0))
    {
        (i = k);
    }
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void UnknownMethodOnMultipleLines()
        {
            var code = @"
void f(int i, int j)
{
    g(i,
    j);
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void ForWithoutCondition()
        {
            var code = @"
void f(int i)
{
    for (;;)
    {
        i = i + 1;
    }
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void ChainingAssignments()
        {
            var code = @"
void f(int i)
{
    int j = i = 0;
    i = j = 10;
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void UseOfReadonlyClassField()
        {
            var code = @"
public class A
{
    readonly int a = 0;

    public int getA()
    {
        return a;
    }
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void UseOfConstantClassField()
        {
            var code = @"
public class A
{
    const int a = 0;

    public int getA()
    {
        return a;
    }
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void UnnamedFunctionParameter()
        {
            var code = @"
int Func(int, int, int i)
{
    return 2*i;
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void SimpleLambdaExpression()
        {
            var code = @"
public System.Linq.Expressions.Expression<Func<int, int>> F()
{
    return x => 2*x;
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void ParenthesizedLambdaExpression()
        {
            var code = @"
Action f()
{
    return () => Y();
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void SwitchStatement()
        {
            var code = @"
int f(int i)
{
    switch (i)
    {
        case 1:
            return 0;
        default:
            return 1;
    }
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void NonASCIIEncodings()
        {
            var code = @"
public void 你好() { }

public void Łódź() { }

public void AsciiThen你好ThenAsciiAgain() { }

public int Łódźअनुلمرadım(int 你好) { return 2 * 你好; }

";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void Arglist()
        {
            var code = @"
public void f(__arglist)
{
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void ParamsKeyword()
        {
            var code = @"
public void f(params int[] args)
        {
        }
";


            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void CheckedUnchecked()
        {
            var code = @"
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
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void Fixed()
        {
            var code = @"
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
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void Namespace()
        {
            var code = @"
namespace Tests
{
    public static void f()
    {
        Tests.g();
    }
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }
        [TestMethod]
        public void Overloads()
        {
            var code = @"
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
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void NestedFunction()
        {
            var code = @"
void f()
{
    var s = g();
    int g()
    {
        return 0;
    }
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void FieldAssignment()
        {
            var code = @"
class Point
{
    public int x;
    public int y;
}

void f(Point p)
{
    p.x = 2;
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void FieldOfThisAssignment()
        {
            var code = @"
class Point
{
    public int x;
    public int y;

    void f(int a)
    {
        this.x = a;
    }
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void ArrayAssignment()
        {
            var code = @"
void f()
{
    int[] array = new int[5];
    array[0] = 2;
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void CastOnReturn()
        {
            var code = @"
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
    }
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void IgnoreParenthesesInAssignment()
        {
            var code = @"
void f(int a, int b)
{
    a = (b = 2);
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void AssignmentInComparison()
        {
            var code = @"
int f(int a, int b)
{
    if ((a = 3) < (b = 2))
    {
        return 0;
    }
    return 1;
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void AssignmentInBinaryOperator()
        {
            var code = @"
void f(int a, int b)
{
    a = (b = 2) + 1;
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void Goto()
        {
            var code = @"
void f()
{
    goto Label;
Label:
    return;
}
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void UnknownConstant()
        {
            var code = @"
class A
{
        private const object NullConst = null;

        void f()
        {
            NullConst.ToString();
        }
";

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void Property()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void AsyncFunction()
        {
            var code = @"
class A {
    async System.Threading.Tasks.Task<int> FuncAsync()
    {
        return 3;
    }
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void MethodGroup()
        {
            var code = @"
public static Func<string, bool> CreateFilter()
{
    return string.IsNullOrEmpty;
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);

        }
        [TestMethod]
        public void MemberFieldAccess()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);

        }

        [TestMethod]
        public void BoolProperty()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);

        }

        [TestMethod]
        public void MethodsCallsOnMultipleLines()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void AnonymousMethodExpression()
        {
            var code = @"
class A
{
    public delegate object anonymousMethod(params object[] args);

    public static anonymousMethod GetAnonymousMethod()
    {
        return delegate (object[] args) { return 0; };
    }
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void AnonymousObjectCreation()
        {
            var code = @"
public int f()
{
    var v = new { a = 108, m = ""Hello"" };
    return v.a;
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void AssignmentInReturn()
        {
            var code = @"
class A
{
    protected int a;

    public int f()
    {
        return a = 1;
    }
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void BooleanConstant()
        {
            var code = @"
public class A
{
    internal const bool myBool = true;

    void f()
    {
        var b = myBool;
    }
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void PropertyFromAnotherAssembly()
        {
            var code = @"

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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void InfiniteLoop()
        {
            var code = @"
int InfiniteLoop(int i) {
    while (true) {
        if (++i == 42) {
            return i;
        }
    }
    // No return here
}
";
            var dot = MlirTestUtilities.GetCfgGraph(code, "InfiniteLoop");

            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void IfDynamic()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void ModifyFieldParameterAndLocalVariable()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void BinaryOperatorAssignment()
        {
            var code = @"
public void f(int i)
{
    i -= 1;
    i += 2;
    i *= 3;
    i /= 4;
    i %= 5;
}

public void g(int[] array1, long[] array2)
{
    array1[0] += 2;
    array2[0] += 3;
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void NoIdentifier()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

        [TestMethod]
        public void IncompatibleAssignmentTypes()
        {
            var code = @"
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
}
";
            MlirTestUtilities.ValidateCodeGeneration(code, TestContext.TestName);
        }

    } // Class

} // Namespace

