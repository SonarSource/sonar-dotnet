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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SonarAnalyzer.Rules.CSharp
{
    public abstract class CbdeHandler : SonarDiagnosticAnalyzer
    {
        private const string cbdeJsonOutputFileName = "cbdeSEout.json";

        protected const string DiagnosticId = "S2583";
        protected const string MessageFormat = "Condition is always {0}";

        private static string cbdeBinaryPath;
        private string mlirDirectoryRoot;
        private string mlirDirectoryAssembly;
        private string cbdeJsonOutputPath;
        private string logFilePath;
        protected HashSet<string> csSourceFileNames= new HashSet<string>();
        protected Dictionary<string, int> fileNameDuplicateNumbering = new Dictionary<string, int>();
        private StreamWriter logFile;

        static CbdeHandler()
        {
            UnpackCbdeExe();
        }
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            if (cbdeBinaryPath != null)
            {
                RegisterMlirAndCbdeInOneStep(context);
            }
        }
        private void RegisterMlirAndCbdeInOneStep(SonarAnalysisContext context)
        {
            context.RegisterCompilationAction(
                c =>
                {
                    Console.WriteLine("CBDE: Compilation phase");

                    InitializePathsAndLog(c.Compilation.Assembly.Name);
                    foreach (var tree in c.Compilation.SyntaxTrees)
                    {
                        csSourceFileNames.Add(tree.FilePath);
                        Console.WriteLine($"CBDE: Treating file {tree.FilePath}");
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
        private static void UnpackCbdeExe()
        {
            Console.WriteLine("Starting to unpack CBDE");
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var rootPath = Path.GetDirectoryName(assembly.Location);
            const string res = "SonarAnalyzer.CBDE.windows.dotnet-symbolic-execution.exe";
            cbdeBinaryPath = Path.Combine(rootPath, "CBDE/windows/dotnet-symbolic-execution.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(cbdeBinaryPath));
            var stream = assembly.GetManifestResourceStream(res);
            var fileStream = File.Create(cbdeBinaryPath);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
            fileStream.Close();
            Console.WriteLine("CBDE unpacked");
        }
        private void InitializePathsAndLog(string assemblyName)
        {
            SetupMlirRootDirectory();
            mlirDirectoryAssembly = Path.Combine(mlirDirectoryRoot, assemblyName);
            cbdeJsonOutputPath = Path.Combine(mlirDirectoryAssembly, cbdeJsonOutputFileName);
            Directory.CreateDirectory(mlirDirectoryAssembly);
            logFilePath = Path.Combine(mlirDirectoryAssembly, "CbdeHandler.log");
            logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(">> New Cbde Run triggered at {0}", DateTime.Now.ToShortTimeString());
            logFile.Flush();
        }
        private void SetupMlirRootDirectory()
        {
            mlirDirectoryRoot = Path.Combine(Path.GetTempPath(), "sonar-dotnet/cbde");
            Directory.CreateDirectory(mlirDirectoryRoot);
        }
        private void ExportFunctionMlir(SyntaxTree tree, SemanticModel model, string mlirFileName)
        {
            using (var streamWriter = new StreamWriter(Path.Combine(mlirDirectoryAssembly, mlirFileName)))
            {
                MLIRExporter mlirExporter = new MLIRExporter(streamWriter, model, true);
                foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    Console.WriteLine($"CBDE: Exporting {method.Identifier.ValueText}");
                    mlirExporter.ExportFunction(method);
                }
            }
        }
        private void RunCbdeAndRaiseIssues(CompilationAnalysisContext c)
        {
            using (Process pProcess = new Process())
            {
                logFile.WriteLine("- Cbde process");
                pProcess.StartInfo.FileName = cbdeBinaryPath;
                pProcess.StartInfo.WorkingDirectory = mlirDirectoryAssembly;
                pProcess.StartInfo.Arguments = "-i " + "\"" + mlirDirectoryAssembly + "\" -o \"" + cbdeJsonOutputPath + "\"";
                logFile.WriteLine("  * binary_location: '{0}'", pProcess.StartInfo.FileName);
                logFile.WriteLine("  * arguments: '{0}'", pProcess.StartInfo.Arguments);
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardError = true;
                pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true;
                logFile.Flush();
                pProcess.Start();
                pProcess.WaitForExit();
                var output = pProcess.StandardOutput.ReadToEnd();
                var eoutput = pProcess.StandardError.ReadToEnd();
                logFile.WriteLine("  * exit_code: '{0}'", pProcess.ExitCode);
                logFile.WriteLine("  * stdout: '{0}'", output);
                logFile.WriteLine("  * stderr: '{0}'", eoutput);
                logFile.Flush();
                // TODO: log this pProcess.PeakWorkingSet64;
                if (pProcess.ExitCode != 0)
                {
                    RaiseIssueFromFailedCbdeRun(c);
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
        private static Diagnostic CreateDiagnosticFromFailureString(string filename, string description)
        {
            var rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, description, RspecStrings.ResourceManager);

            var begin = new LinePosition(1,1);
            var end = new LinePosition(1,2);
            var loc = Location.Create(filename, TextSpan.FromBounds(0, 0), new LinePositionSpan(begin, end));

            return Diagnostic.Create(rule, loc);
        }
        private void RaiseIssueFromFailedCbdeRun(CompilationAnalysisContext context)
        {            
            StringBuilder failureString = new StringBuilder("CBDE Failure Report :\n  C# souces files involved are:\n");
            foreach (var fileName in csSourceFileNames)
            {
                failureString.Append("  - " + fileName + "\n");
            }
            // we dispose the StreamWriter to unlock the log file
            logFile.Dispose();
            failureString.Append("  content of the CBDE handler log file is :\n" + File.ReadAllText(logFilePath));
            logFile = new StreamWriter(logFilePath, true);
            foreach (var fileName in csSourceFileNames)
            {
                context.ReportDiagnosticWhenActive(CreateDiagnosticFromFailureString(fileName, failureString.ToString()));
            }
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
    public sealed class CbdeHandlerRule : CbdeHandler
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
    }
}
