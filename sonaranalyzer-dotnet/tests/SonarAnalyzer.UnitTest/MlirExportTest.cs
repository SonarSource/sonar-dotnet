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
                Arguments = path,
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

        public TestContext TestContext { get; set; } // Set automatically by MsTest
    }
}

