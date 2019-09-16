extern alias csharp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.ControlFlowGraph;

namespace SonarAnalyzer.UnitTest.CBDE
{
    public static class MlirTestUtilities
    {
        private static string cbdeDialectCheckerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CBDE\windows\cbde-dialect-checker.exe");
        public static void checkExecutableExists()
        {
            Assert.IsTrue(File.Exists(cbdeDialectCheckerPath),
                $"We need cbde-dialect-checker.exe to validate the generated IR, searched in path {cbdeDialectCheckerPath}");
        }

        private static string ValidateIR(string path)
        {
            var pi = new ProcessStartInfo
            {
                FileName = cbdeDialectCheckerPath,
                Arguments = '"' + path + '"',
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var p = Process.Start(pi);
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                Assert.Fail(p.StandardError.ReadToEnd());
                return string.Empty;
            }
            else
            {
                return p.StandardOutput.ReadToEnd();
            }
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
            foreach (var method in ast.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                exporter.ExportFunction(method);
            }

        }
        public static string ValidateCodeGeneration(string code, string testName, bool withLoc)
        {
            var locPath = withLoc ? ".loc" : "";
            var path = Path.Combine(Path.GetTempPath(), $"csharp.{testName}{locPath}.mlir");
            using (var writer = new StreamWriter(path))
            {
                ExportAllMethods(code, writer, withLoc);
            }
            return ValidateIR(path);
        }

        public static void ValidateCodeGeneration(string code, string testName)
        {
            ValidateCodeGeneration(code, testName, false);
            ValidateCodeGeneration(code, testName, true);
        }

        public static void ValidateWithReference(string code, string expected, string testName)
        {
            var actual = ValidateCodeGeneration(code, testName, false);
            Assert.AreEqual(expected.Trim(), actual.Trim());
        }

        public static IControlFlowGraph GetCfgForMethod(string code, string methodName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code).GetMethod(methodName);

            return CSharpControlFlowGraph.Create(method.Body, semanticModel);
        }
        public static string GetCfgGraph(string code, string methodName)
        {
            return CfgSerializer.Serialize(methodName, GetCfgForMethod(code, methodName));
        }
    }
}
