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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SonarAnalyzer.Rules.CSharp
{
    public class CbdeHandler
    {
        private readonly Action<String, String, Location, CompilationAnalysisContext> raiseIssue;
        private readonly Func<CompilationAnalysisContext, bool> shouldRunInContext;
        private static bool initialized = false;

        private const string cbdeJsonOutputFileName = "cbdeSEout.json";

        private static string cbdeBinaryPath;
        private string mlirDirectoryRoot;
        private string mlirDirectoryAssembly;
        private string cbdeJsonOutputPath;

        protected HashSet<string> csSourceFileNames= new HashSet<string>();
        protected Dictionary<string, int> fileNameDuplicateNumbering = new Dictionary<string, int>();

        private MemoryStream logStream;
        private StreamWriter logStreamWriter;
        private static readonly object logFileLock = new Object();
        private static readonly object metricsFileLock = new Object();
        private static readonly object perfFileLock = new Object();
        private static readonly object staticInitLock = new Object();

        private static readonly string mlirPath =
            Environment.GetEnvironmentVariable("CIRRUS_WORKING_DIR") ?? // For Cirrus
            Environment.GetEnvironmentVariable("WORKSPACE") ?? // For Jenkins
            Path.GetTempPath(); // By default
        private static readonly string mlirProcessSpecificPath = Path.Combine(mlirPath, $"CBDE_{Process.GetCurrentProcess().Id}");
        private static readonly string mlirLogFile = Path.Combine(mlirProcessSpecificPath, "cbdeHandler.log");
        private static readonly string mlirMetricsLogFile = Path.Combine(mlirProcessSpecificPath, "metrics.log");
        private static readonly string mlirPerfLogFile = Path.Combine(mlirProcessSpecificPath, "performances.log");
        private static void GlobalLog(string s)
        {
            lock (logFileLock)
            {
                var message = $"{DateTime.Now} ({Thread.CurrentThread.ManagedThreadId,5}): {s}\n";
                File.AppendAllText(mlirLogFile, message);
            }
        }

        private void PerformanceLog(string s)
        {
            lock (perfFileLock)
            {
                File.AppendAllText(mlirPerfLogFile, s);
            }
        }

        private static void Initialize()
        {
            Directory.CreateDirectory(mlirProcessSpecificPath);
            lock (logFileLock)
            {
                if (File.Exists(mlirProcessSpecificPath))
                {
                    File.Delete(mlirProcessSpecificPath);
                }
            }
            GlobalLog("Unpacking CBDE executables");
            UnpackCbdeExe();
        }
        public CbdeHandler(SonarAnalysisContext context,
            Action<String, String, Location, CompilationAnalysisContext> raiseIssue,
            Func<CompilationAnalysisContext, bool> shouldRunInContext)            
        {
            this.raiseIssue = raiseIssue;
            this.shouldRunInContext = shouldRunInContext;
            lock (staticInitLock)
            {
                if(!initialized)
                {
                    Initialize();
                    initialized = true;
                }
            }
            GlobalLog("Before initialize");
            var watch = Stopwatch.StartNew();
            if (cbdeBinaryPath != null)
            {
                RegisterMlirAndCbdeInOneStep(context);
            }
            watch.Stop();
            GlobalLog($"After initialize ({watch.ElapsedMilliseconds} ms)");
        }
        private void RegisterMlirAndCbdeInOneStep(SonarAnalysisContext context)
        {
            context.RegisterCompilationAction(
                c =>
                {
                    if(!shouldRunInContext(c))
                    {
                        return;
                    }
                    var compilationHash = c.Compilation.GetHashCode();
                    InitializePathsAndLog(c.Compilation.Assembly.Name, compilationHash);
                    GlobalLog("CBDE: Compilation phase");
                    var exporterMetrics = new MlirExporterMetrics();
                    try
                    {
                        var watch = Stopwatch.StartNew();
                        foreach (var tree in c.Compilation.SyntaxTrees)
                        {
                            csSourceFileNames.Add(tree.FilePath);
                            GlobalLog($"CBDE: Processing file {tree.FilePath} in context {compilationHash}");
                            var mlirFileName = ManglePath(tree.FilePath) + ".mlir";
                            ExportFunctionMlir(tree, c.Compilation.GetSemanticModel(tree), exporterMetrics, mlirFileName);
                            logStreamWriter.WriteLine("- generated mlir file {0}", mlirFileName);
                            logStreamWriter.Flush();
                            GlobalLog($"CBDE: Done with file {tree.FilePath} in context {compilationHash}");
                        }
                        GlobalLog($"CBDE: MLIR generation time: {watch.ElapsedMilliseconds} ms");
                        watch.Restart();
                        RunCbdeAndRaiseIssues(c);
                        GlobalLog($"CBDE: CBDE execution and reporting time: {watch.ElapsedMilliseconds} ms");
                        GlobalLog("CBDE: End of compilation");
                        lock (metricsFileLock)
                        {
                            File.AppendAllText(mlirMetricsLogFile, exporterMetrics.Dump());
                        }
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
            const string res = "SonarAnalyzer.CBDE.windows.dotnet-symbolic-execution.exe";
            cbdeBinaryPath = Path.Combine(mlirProcessSpecificPath, "windows/dotnet-symbolic-execution.exe");
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
            if (Directory.Exists(mlirDirectoryAssembly))
            {
                Directory.Delete(mlirDirectoryAssembly, true);
            }
            Directory.CreateDirectory(mlirDirectoryAssembly);
            cbdeJsonOutputPath = Path.Combine(mlirDirectoryAssembly, cbdeJsonOutputFileName);
            logStream = new MemoryStream();
            logStreamWriter = new StreamWriter(logStream);
            logStreamWriter.WriteLine(">> New Cbde Run triggered at {0}", DateTime.Now.ToShortTimeString());
            logStreamWriter.Flush();
        }
        private void SetupMlirRootDirectory()
        {
            mlirDirectoryRoot = Path.Combine(mlirPath, "sonar-dotnet/cbde");
            Directory.CreateDirectory(mlirDirectoryRoot);
        }
        private void ExportFunctionMlir(SyntaxTree tree, SemanticModel model, MlirExporterMetrics exporterMetrics, string mlirFileName)
        {
            using (var mlirStreamWriter = new StreamWriter(Path.Combine(mlirDirectoryAssembly, mlirFileName)))
            {
                string perfLog = tree.GetRoot().GetLocation().GetLineSpan().Path + "\n";
                MLIRExporter mlirExporter = new MLIRExporter(mlirStreamWriter, model, exporterMetrics, true);
                foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    mlirExporter.ExportFunction(method);
                    watch.Stop();

                    perfLog += method.Identifier + " " + watch.ElapsedMilliseconds + "\n";
                }
                PerformanceLog(perfLog + "\n");
            }
        }
        private void RunCbdeAndRaiseIssues(CompilationAnalysisContext c)
        {
            GlobalLog("Running CBDE");
            using (Process pProcess = new Process())
            {
                logStreamWriter.WriteLine("- Cbde process");
                pProcess.StartInfo.FileName = cbdeBinaryPath;
                pProcess.StartInfo.WorkingDirectory = mlirDirectoryAssembly;
                var progressLogFile = Path.Combine(mlirDirectoryAssembly, "progressLogFile.log");
                var cbdePerfLogFile = Path.Combine(mlirDirectoryAssembly, "perfLogFile.log");

                pProcess.StartInfo.Arguments = "-i " + "\"" + mlirDirectoryAssembly + "\" -o \"" + cbdeJsonOutputPath + "\" -p \"" + progressLogFile + "\" -s \"" + cbdePerfLogFile + "\"";

                logStreamWriter.WriteLine("  * binary_location: '{0}'", pProcess.StartInfo.FileName);
                logStreamWriter.WriteLine("  * arguments: '{0}'", pProcess.StartInfo.Arguments);
                logStreamWriter.Flush();

                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardError = true;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.Start();
                pProcess.WaitForExit();

                logStreamWriter.WriteLine("  * exit_code: '{0}'", pProcess.ExitCode);
                logStreamWriter.Flush();

                if (pProcess.ExitCode != 0)
                {
                    GlobalLog("Running CBDE: Failure");
                    LogFailedCbdeRun(pProcess);
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
            logStreamWriter.Dispose();
        }
        private void RaiseIssueFromJToken(JToken token, CompilationAnalysisContext context)
        {
            var key = token["key"].ToString();
            var message = token["message"].ToString();
            var location = token["location"];
            var line = Convert.ToInt32(location["l"]);
            var col = Convert.ToInt32(location["c"]);
            var file = location["f"].ToString();

            var begin = new LinePosition(line, col);
            var end = new LinePosition(line, col + 1);
            var loc = Location.Create(file, TextSpan.FromBounds(0, 0), new LinePositionSpan(begin, end));

            raiseIssue(key, message, loc, context);
        }
        private void LogFailedCbdeRun(Process pProcess)
        {
            StringBuilder failureString = new StringBuilder("CBDE Failure Report :\n  C# souces files involved are:\n");
            foreach (var fileName in csSourceFileNames)
            {
                failureString.Append("  - " + fileName + "\n");
            }
            // we dispose the StreamWriter to unlock the log file
            logStreamWriter.WriteLine("- parsing json file {0}", cbdeJsonOutputPath);
            failureString.Append("  content of stderr is:\n" + pProcess.StandardError.ReadToEnd());
            failureString.Append("  content of the CBDE handler log file is :\n" + Encoding.UTF8.GetString(logStream.GetBuffer()));
            GlobalLog(failureString.ToString());
            Console.Error.WriteLine($"Error when executing MLIR, more details in {mlirProcessSpecificPath}");
        }
        private void RaiseIssuesFromJSon(CompilationAnalysisContext context)
        {
            string jsonFileContent;
            List<List<JObject>> jsonIssues;
            logStreamWriter.WriteLine("- parsing json file {0}", cbdeJsonOutputPath);
            try
            {
                jsonFileContent = File.ReadAllText(cbdeJsonOutputPath);
                jsonIssues = JsonConvert.DeserializeObject<List<List<JObject>>>(jsonFileContent);
            }
            catch
            {
                logStreamWriter.WriteLine("- error parsing json file {0}", cbdeJsonOutputPath);
                return;
            }

            foreach (var issue in jsonIssues.First())
            {
                logStreamWriter.WriteLine("  * processing token {0}", issue.ToString());
                try
                {
                    RaiseIssueFromJToken(issue, context);
                }
                catch
                {
                    logStreamWriter.WriteLine("  * error reporting token {0}", cbdeJsonOutputPath);
                    continue;
                }
            }
            logStreamWriter.Flush();
        }
    }
}
