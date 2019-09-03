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
using System.Threading;

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
        private MemoryStream logStream;
        private StreamWriter logFile;
        private static readonly object logFileLock = new Object();

        private static readonly string mlirPath =
            Environment.GetEnvironmentVariable("CIRRUS_WORKING_DIR") ?? Path.GetTempPath();
        private static readonly string mlirGlobalLogPath =
            Path.Combine(mlirPath, $"CBDE_{Process.GetCurrentProcess().Id}.log");
        private static void GlobalLog(string s)
        {
            lock (logFileLock)
            {
                var message = $"{DateTime.Now} ({Thread.CurrentThread.ManagedThreadId,5}): {s}\n";
                File.AppendAllText(mlirGlobalLogPath, message);
            }
        }

        static CbdeHandler()
        {
            lock (logFileLock)
            {
                if (File.Exists(mlirGlobalLogPath))
                {
                    File.Delete(mlirGlobalLogPath);
                }
            }
            GlobalLog("Before unpack");
            UnpackCbdeExe();
            GlobalLog("After unpack");
        }
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            GlobalLog("Before initialize");
            if (cbdeBinaryPath != null)
            {
                RegisterMlirAndCbdeInOneStep(context);
            }
            GlobalLog("After initialize");
        }
        private void RegisterMlirAndCbdeInOneStep(SonarAnalysisContext context)
        {
            context.RegisterCompilationAction(
                c =>
                {
                    var compilationHash = c.Compilation.GetHashCode();
                    InitializePathsAndLog(c.Compilation.Assembly.Name, compilationHash);
                    GlobalLog("CBDE: Compilation phase");
                    try
                    {
                        foreach (var tree in c.Compilation.SyntaxTrees)
                        {
                            csSourceFileNames.Add(tree.FilePath);
                            GlobalLog($"CBDE: Treating file {tree.FilePath} in context {compilationHash}");
                            var mlirFileName = ManglePath(tree.FilePath) + ".mlir";
                            ExportFunctionMlir(tree, c.Compilation.GetSemanticModel(tree), mlirFileName);
                            logFile.WriteLine("- generated mlir file {0}", mlirFileName);
                            logFile.Flush();
                            GlobalLog($"CBDE: Done with file {tree.FilePath} in context {compilationHash}");
                        }
                        RunCbdeAndRaiseIssues(c);
                        GlobalLog("CBDE: End of compilation");
                    }
                    catch(Exception e)
                    {
                        GlobalLog("An exception has occured: " + e.Message + "\n" + e.StackTrace);
                        throw;
                    }
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
        }
        private void InitializePathsAndLog(string assemblyName, int compilationHash)
        {
            SetupMlirRootDirectory();
            mlirDirectoryAssembly = Path.Combine(mlirDirectoryRoot, assemblyName, compilationHash.ToString());
            Directory.CreateDirectory(mlirDirectoryAssembly);
            cbdeJsonOutputPath = Path.Combine(mlirDirectoryAssembly, cbdeJsonOutputFileName);
            logFilePath = Path.Combine(mlirDirectoryAssembly, "CbdeHandler.log");
            logStream = new MemoryStream();
            logFile = new StreamWriter(logStream);
            logFile.WriteLine(">> New Cbde Run triggered at {0}", DateTime.Now.ToShortTimeString());
            logFile.Flush();
        }
        private void DumpLogToLogFile()
        {
            var str = UTF8Encoding.UTF8.GetString(logStream.GetBuffer());
            File.AppendAllText(logFilePath, str, Encoding.UTF8);
        }
        private void SetupMlirRootDirectory()
        {
            mlirDirectoryRoot = Path.Combine(mlirPath, "sonar-dotnet/cbde");
            Directory.CreateDirectory(mlirDirectoryRoot);
        }
        private void ExportFunctionMlir(SyntaxTree tree, SemanticModel model, string mlirFileName)
        {
            using (var streamWriter = new StreamWriter(Path.Combine(mlirDirectoryAssembly, mlirFileName)))
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
            GlobalLog("Running CBDE");
            using (Process pProcess = new Process())
            {
                logFile.WriteLine("- Cbde process");
                pProcess.StartInfo.FileName = cbdeBinaryPath;
                pProcess.StartInfo.WorkingDirectory = mlirDirectoryAssembly;
                pProcess.StartInfo.Arguments = "-i " + "\"" + mlirDirectoryAssembly + "\" -o \"" + cbdeJsonOutputPath + "\"";
                logFile.WriteLine("  * binary_location: '{0}'", pProcess.StartInfo.FileName);
                logFile.WriteLine("  * arguments: '{0}'", pProcess.StartInfo.Arguments);
                pProcess.StartInfo.UseShellExecute = false;
                //pProcess.StartInfo.RedirectStandardOutput = true;
                //pProcess.StartInfo.RedirectStandardError = true;
                //pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //pProcess.StartInfo.CreateNoWindow = true;
                logFile.Flush();
                pProcess.Start();
                pProcess.WaitForExit();
                //var output = pProcess.StandardOutput.ReadToEnd();
                //var eoutput = pProcess.StandardError.ReadToEnd();
                logFile.WriteLine("  * exit_code: '{0}'", pProcess.ExitCode);
                //logFile.WriteLine("  * stdout: '{0}'", output);
                //logFile.WriteLine("  * stderr: '{0}'", eoutput);
                logFile.Flush();
                // TODO: log this pProcess.PeakWorkingSet64;
                if (pProcess.ExitCode != 0)
                {
                    GlobalLog("Running CBDE: Failure");
                    RaiseIssueFromFailedCbdeRun(c);
                    DumpLogToLogFile();
                    GlobalLog("Running CBDE: Error dumped");
                }
                else
                {
                    GlobalLog("Running CBDE: Success");
                    RaiseIssuesFromJSon(c);
                    Cleanup();
                    GlobalLog("Running CBDE: Issues reported");
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
            failureString.Append("  content of the CBDE handler log file is :\n" + Encoding.UTF8.GetString(logStream.GetBuffer()));
            GlobalLog(failureString.ToString());
            Console.Error.WriteLine("Error when executing MLIR");
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
