extern alias csharp;

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.ControlFlowGraph;

namespace SonarAnalyzer.UnitTest
{
    [TestClass]
    public class MlirExportTest
    {
        private static string mlirCheckerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mlir-cbde.exe");
        [ClassInitialize]
        public static void checkExecutableExists(TestContext tc)
        {
            Assert.IsTrue(File.Exists(mlirCheckerPath), "We need mlir-cbde.exe to validate the generated IR");
        }

        public static void ValidateIR(string path)
        {
            var pi = new ProcessStartInfo
            {
                FileName = mlirCheckerPath,
                Arguments = '"' + path + '"',
                UseShellExecute = false,
                // RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var p =Process.Start(pi);
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                Assert.Fail(p.StandardError.ReadToEnd());
            }
        }

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
            ValidateCodeGeneration(code);
        }

        private static void ExportMethod(string code, TextWriter writer, string functionName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code).GetMethod(functionName);
            var exporter = new MLIRExporter(writer, semanticModel, false);
            exporter.ExportFunction(method);
        }

        private static void ExportAllMethods(string code, TextWriter writer, bool withLoc)
        {
            (var ast, var semanticModel) = TestHelper.Compile(code);
            var exporter = new MLIRExporter(writer, semanticModel, withLoc);
            foreach(var method in ast.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                exporter.ExportFunction(method);
            }

        }
        private void ValidateCodeGeneration(string code, bool withLoc)
        {
            var locPath = withLoc ? ".loc" : "";
            var path = Path.Combine(Path.GetTempPath(), $"csharp.{TestContext.TestName}{locPath}.mlir");
            using (var writer = new StreamWriter(path))
            {
                ExportAllMethods(code, writer, withLoc);
            }
            ValidateIR(path);
        }

        private void ValidateCodeGeneration(string code)
        {
            ValidateCodeGeneration(code, false);
            ValidateCodeGeneration(code, true);
        }
        protected IControlFlowGraph GetCfgForMethod(string code, string methodName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code).GetMethod(methodName);

            return CSharpControlFlowGraph.Create(method.Body, semanticModel);
        }


        public TestContext TestContext { get; set; } // Set automatically by MsTest

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
            var dot = GetCfgGraph(code, "WithReturn");
            ValidateCodeGeneration(code);
        }

        private string GetCfgGraph(string code, string methodName)
        {
            return CfgSerializer.Serialize(methodName, GetCfgForMethod(code, methodName));
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
            ValidateCodeGeneration(code);

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
            ValidateCodeGeneration(code);

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
            ValidateCodeGeneration(code);

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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            var dot = GetCfgGraph(code, "TryThrow");
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            var dot = GetCfgGraph(code, "f");

            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void EmptyBody()
        {
            var code = @"
public static extern void Extern(int p1);
";
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            var dot = GetCfgGraph(code, "f");

            ValidateCodeGeneration(code);
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
            var dot = GetCfgGraph(code, "f");

            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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
            ValidateCodeGeneration(code);
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

            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void Arglist()
        {
            var code = @"
public void f(__arglist)
{
}
";

            ValidateCodeGeneration(code);
        }

        [TestMethod]
        public void ParamsKeyword()
        {
            var code = @"
public void f(params int[] args)
        {
        }
";


            ValidateCodeGeneration(code);
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

            ValidateCodeGeneration(code);
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

            ValidateCodeGeneration(code);
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

            ValidateCodeGeneration(code);
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

            ValidateCodeGeneration(code);
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

            ValidateCodeGeneration(code);
        }

    } // Class
} // Namespace


