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
using SonarAnalyzer.UnitTest.TestFramework;

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
            ExportMlirFromTest.ValidateIR(path);
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

    } // Class
} // Namespace


