/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Linq;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using SonarAnalyzer.Common;
using System.Collections.Generic;
using System.Diagnostics;

namespace SonarAnalyzer.Rules.CSharp
{
    public abstract class CbdeHandler : SonarDiagnosticAnalyzer
    {
        private const string cbdeSymbExecBinaryPath = "c:\\Temp\\dotnet-symbolic-execution.exe";
        private const string mlirDirectoryRoot = "c:\\Temp\\";
        private const string cbdeJsonOutputFileName = "cbdeSEout.json";

        protected const string DiagnosticId = "S2583";
        protected const string MessageFormat = "Condition is always {0}";

        private string mlirDirectoryAssembly;
        private string cbdeJsonOutputPath;
        protected Dictionary<string, int> fileNameDuplicateNumbering = new Dictionary<string, int>();
        private StreamWriter logFile;

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            RegisterMlirAndCbdeInOneStep(context);
        }
        private void RegisterMlirAndCbdeInOneStep(SonarAnalysisContext context)
        {
            context.RegisterCompilationAction(
                c =>
                {
                    mlirDirectoryAssembly = Path.Combine(mlirDirectoryRoot, c.Compilation.Assembly.Name);
                    cbdeJsonOutputPath = Path.Combine(mlirDirectoryAssembly, cbdeJsonOutputFileName);
                    Directory.CreateDirectory(mlirDirectoryAssembly);
                    logFile = new StreamWriter(Path.Combine(mlirDirectoryAssembly, "CbdeHandler.log"), true);
                    logFile.WriteLine(">> New Cbde Run triggered at {0}", DateTime.Now.ToShortTimeString());
                    logFile.Flush();
                    foreach (var tree in c.Compilation.SyntaxTrees)
                    {
                        var mlirFileName = ManglePath(tree.FilePath) + ".mlir";
                        ExportFunctionMlir(tree, c.Compilation.GetSemanticModel(tree), mlirFileName);
                        logFile.WriteLine("- generated mlir file {0}", mlirFileName);
                        logFile.Flush();
                    }
                    RunCbdeAndRaiseIssues(c);
                });
        }
        private string ManglePath(string path)
        {
            path = Path.GetFileNameWithoutExtension(path);
            int count = 0;
            fileNameDuplicateNumbering.TryGetValue(path, out count);
            fileNameDuplicateNumbering[path] = ++count;
            path += "_" + Convert.ToString(count);
            return path;
        }
        private void ExportFunctionMlir(SyntaxTree tree, SemanticModel model, string mlirFileName)
        {
            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(mlirDirectoryAssembly, mlirFileName)))
            {
                MLIRExporter mlirExporter = new MLIRExporter(streamWriter, model, true);
                foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    mlirExporter.ExportFunction(method);
                }
            }
        }
        private void RunCbdeAndRaiseIssues(CompilationAnalysisContext c)
        {
            using (Process pProcess = new Process())
            {
                logFile.WriteLine("- Cbde process");
                pProcess.StartInfo.FileName = cbdeSymbExecBinaryPath;
                pProcess.StartInfo.WorkingDirectory = mlirDirectoryAssembly;
                pProcess.StartInfo.Arguments = "-i " + "\"" + mlirDirectoryAssembly + "\" -o \"" + cbdeJsonOutputPath + "\"";
                logFile.WriteLine("  * arguments: '{0}'", pProcess.StartInfo.Arguments);
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardError = true;
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true;
                pProcess.Start();
                pProcess.WaitForExit();
                string output = pProcess.StandardOutput.ReadToEnd();
                string eoutput = pProcess.StandardError.ReadToEnd();
                logFile.WriteLine("  * exit_code: '{0}'", pProcess.ExitCode);
                logFile.WriteLine("  * stdout: '{0}'", output);
                logFile.WriteLine("  * stderr: '{0}'", eoutput);
                logFile.Flush();
                // TODO: log this pProcess.PeakWorkingSet64;
                if (pProcess.ExitCode != 0)
                {
                    // rerun with logging and do not cleanup the directory
                    pProcess.StartInfo.Arguments += " -p \"" + Path.Combine(mlirDirectoryAssembly, "CbdeSymbolicExecution.log") + "\"";
                    pProcess.Start();
                    pProcess.WaitForExit();
                }
                else
                {
                    RaiseIssuesFromJSon(c);
                    Cleanup();
                }
            }
        }
        private void Cleanup()
        {
            logFile.Dispose();
            Directory.Delete(mlirDirectoryAssembly, true);
        }
        private static Diagnostic CreateDiagnosticFromJToken(JToken token)
        {
            var key = token["key"].ToString();
            var message = token["message"].ToString();
            var location = token["location"];
            var line = Convert.ToInt32(location["l"]);
            var col = Convert.ToInt32(location["c"]);
            var file = location["f"].ToString();

            DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(key, message, RspecStrings.ResourceManager);
            var begin = new LinePosition(line, col);
            var end = new LinePosition(line, col + 1);
            var loc = Location.Create(file, TextSpan.FromBounds(0, 0), new LinePositionSpan(begin, end));

            return Diagnostic.Create(rule, loc);
        }
        private void RaiseIssuesFromJSon(CompilationAnalysisContext context)
        {
            string jsonFileContent;
            List<List<JObject>> jsonIssues;
            logFile.WriteLine("- parsing json file {0}", cbdeJsonOutputPath);
            try
            {
                jsonFileContent = File.ReadAllText(cbdeJsonOutputPath);
                jsonIssues = JsonConvert.DeserializeObject<List<List<JObject>>>(jsonFileContent);
            }
            catch
            {
                logFile.WriteLine("- error parsing json file {0}", cbdeJsonOutputPath);
                return;
            }

            foreach (var issue in jsonIssues.First())
            {
                logFile.WriteLine("  * processing token {0}", issue.ToString());
                try
                {
                    context.ReportDiagnosticWhenActive(CreateDiagnosticFromJToken(issue));
                }
                catch
                {
                    logFile.WriteLine("  * error reporting token {0}", cbdeJsonOutputPath);
                    continue;
                }
            }
            logFile.Flush();
        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CbdeHandlerRule : CbdeHandler
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
    }
}
