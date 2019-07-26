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
            var exporter = new MLIRExporter(writer, semanticModel);
            exporter.ExportFunction(method);
        }

        private static void ExportAllMethods(string code, TextWriter writer)
        {
            (var ast, var semanticModel) = TestHelper.Compile(code);
            foreach(var method in ast.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var exporter = new MLIRExporter(writer, semanticModel);
                exporter.ExportFunction(method);
            }

        }

        private void ValidateCodeGeneration(string code)
        {
            var path = Path.Combine(Path.GetTempPath(), $"csharp.{TestContext.TestName}.mlir");
            using (var writer = new StreamWriter(path))
            {
                ExportAllMethods(code, writer);
            }
            ValidateIR(path);
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
    }
}

