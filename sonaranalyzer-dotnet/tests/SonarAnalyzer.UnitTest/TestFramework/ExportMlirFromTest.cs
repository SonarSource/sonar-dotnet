using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal static class ExportMlirFromTest
    {
        private static string mlirCheckerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mlir-cbde.exe");

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
            var p = Process.Start(pi);
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                Assert.Fail(p.StandardError.ReadToEnd());
            }
        }

        public static void exportMlir(Compilation compilation, string id)
        {
            var path = Path.Combine(Path.GetTempPath(), $"csharp.{id}.mlir");
            using (var writer = new StreamWriter(path))
            {
                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var semanticModel = compilation.GetSemanticModel(syntaxTree);
                    foreach (var method in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                    {
                        var exporter = new MLIRExporter(writer, semanticModel, false);
                        exporter.ExportFunction(method);
                    }
                }
            }
            ValidateIR(path);
        }
    }
}
