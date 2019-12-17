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
        private readonly Func<string> getOutputDirectory;
        private static bool initialized = false;
        private const int processStatPeriodMs = 1000;

        private const string cbdeJsonOutputFileName = "cbdeSEout.json";

        // this is the place where the cbde executable is unpacked. It is in a temp folder
        private static string cbdeBinaryPath;

        protected HashSet<string> csSourceFileNames= new HashSet<string>();
        protected Dictionary<string, int> fileNameDuplicateNumbering = new Dictionary<string, int>();

        private StringBuilder logStringBuilder;
        private static readonly object logFileLock = new Object();
        private static readonly object metricsFileLock = new Object();
        private static readonly object perfFileLock = new Object();
        private static readonly object staticInitLock = new Object();

        // cbdePath is inside .sonarqube/out/<n>/
        // cbdeDirectoryRoot contains mlir files and results for each assembly
        // cbdeProcessSpecificPath is the $"{cbdePath}/CBDE_{pid}/" folder
        // cbdeLogFile, cbdeMetricsLogFile and cbdePerfLogFile are inside cbdeProcessSpecificPath
        private string cbdePath;
        private string cbdeDirectoryRoot;
        private string cbdeDirectoryAssembly;
        private string cbdeJsonOutputPath;
        private static string cbdeProcessSpecificPath;
        private string cbdeLogFile;
        private string cbdeMetricsLogFile;
        private string cbdePerfLogFile;
        private bool emitLog = false;

        private void LogIfFailure(string s)
        {
            if (emitLog)
            {
                logStringBuilder.AppendLine(s);
            }
        }

        private void Log(string s)
        {
            if(emitLog)
            {
                lock (logFileLock)
                {
                    var message = $"{DateTime.Now} ({Thread.CurrentThread.ManagedThreadId,5}): {s}\n";
                    File.AppendAllText(cbdeLogFile, message);
                }
            }
        }

        private void PerformanceLog(string s)
        {
            lock (perfFileLock)
            {
                File.AppendAllText(cbdePerfLogFile, s);
            }
        }

        private static void Initialize()
        {
            cbdeBinaryPath = Path.Combine(Path.GetTempPath(), $"CBDE_{Process.GetCurrentProcess().Id}");
            Directory.CreateDirectory(cbdeBinaryPath);
            lock (logFileLock)
            {
                if (File.Exists(cbdeBinaryPath))
                {
                    File.Delete(cbdeBinaryPath);
                }
            }
            UnpackCbdeExe();
        }

        public CbdeHandler(SonarAnalysisContext context,
            Action<String, String, Location, CompilationAnalysisContext> raiseIssue,
            Func<CompilationAnalysisContext, bool> shouldRunInContext,
            Func<string> getOutputDirectory)
        {
            this.raiseIssue = raiseIssue;
            this.shouldRunInContext = shouldRunInContext;
            this.getOutputDirectory = getOutputDirectory;
            lock (staticInitLock)
            {
                if(!initialized)
                {
                    Initialize();
                    initialized = true;
                }
            }
            Log("Before initialize");
            var watch = Stopwatch.StartNew();
            if (cbdeBinaryPath != null)
            {
                RegisterMlirAndCbdeInOneStep(context);
            }
            watch.Stop();
            Log($"After initialize ({watch.ElapsedMilliseconds} ms)");
        }

        private void RegisterMlirAndCbdeInOneStep(SonarAnalysisContext context)
        {
            context.RegisterCompilationAction(
                c =>
                {
                    emitLog = Environment.GetEnvironmentVariables().Contains("SONAR_DOTNET_INTERNAL_LOG_CBDE");
                    if (!shouldRunInContext(c))
                    {
                        return;
                    }
                    var compilationHash = c.Compilation.GetHashCode();
                    InitializePathsAndLog(c.Compilation.Assembly.Name, compilationHash);
                    Log("CBDE: Compilation phase");
                    var exporterMetrics = new MlirExporterMetrics();
                    try
                    {
                        var watch = Stopwatch.StartNew();
                        var cpuWatch = ThreadCpuStopWatch.StartNew();
                        foreach (var tree in c.Compilation.SyntaxTrees)
                        {
                            csSourceFileNames.Add(tree.FilePath);
                            Log($"CBDE: Generating MLIR for source file {tree.FilePath} in context {compilationHash}");
                            var mlirFileName = ManglePath(tree.FilePath) + ".mlir";
                            ExportFunctionMlir(tree, c.Compilation.GetSemanticModel(tree), exporterMetrics, mlirFileName);
                            LogIfFailure($"- generated mlir file {mlirFileName}");
                            Log($"CBDE: Done with file {tree.FilePath} in context {compilationHash}");
                        }
                        Log($"CBDE: MLIR generation time: {watch.ElapsedMilliseconds} ms");
                        Log($"CBDE: MLIR generation cpu time: {cpuWatch.ElapsedMilliseconds} ms");
                        watch.Restart();
                        RunCbdeAndRaiseIssues(c);
                        Log($"CBDE: CBDE execution and reporting time: {watch.ElapsedMilliseconds} ms");
                        Log($"CBDE: CBDE execution and reporting cpu time: {cpuWatch.ElapsedMilliseconds} ms");
                        Log("CBDE: End of compilation");
                        lock (metricsFileLock)
                        {
                            File.AppendAllText(cbdeMetricsLogFile, exporterMetrics.Dump());
                        }
                    }
                    catch(Exception e)
                    {
                        Log("An exception has occured: " + e.Message + "\n" + e.StackTrace);
                        throw;
                    }
                });
        }

        // In big projects, multiple source files can have the same name.
        // We need to convert all of them to mlir. Mangling the full pathname of each file would be too long.
        // We just give a number to each file haviong the same name.
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
            cbdeBinaryPath = Path.Combine(cbdeBinaryPath, "windows/dotnet-symbolic-execution.exe");
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
            cbdeDirectoryAssembly = Path.Combine(cbdeDirectoryRoot, assemblyName, compilationHash.ToString());
            if (Directory.Exists(cbdeDirectoryAssembly))
            {
                Directory.Delete(cbdeDirectoryAssembly, true);
            }
            Directory.CreateDirectory(cbdeDirectoryAssembly);
            cbdeJsonOutputPath = Path.Combine(cbdeDirectoryAssembly, cbdeJsonOutputFileName);
            logStringBuilder = new StringBuilder();
            LogIfFailure($">> New Cbde Run triggered at {DateTime.Now.ToShortTimeString()}");
        }

        private void SetupMlirRootDirectory()
        {
            if((getOutputDirectory() != null) && (getOutputDirectory().Length != 0))
            {
                cbdePath = Path.Combine(getOutputDirectory(), "cbde");
                Directory.CreateDirectory(cbdePath);
            }
            else
            {
                // used only when doing the unit test
                cbdePath = Path.GetTempPath();
            }
            cbdeProcessSpecificPath = Path.Combine(cbdePath, $"CBDE_{Process.GetCurrentProcess().Id}");
            Directory.CreateDirectory(cbdeProcessSpecificPath);
            cbdeLogFile = Path.Combine(cbdeProcessSpecificPath, "cbdeHandler.log");
            cbdeMetricsLogFile = Path.Combine(cbdeProcessSpecificPath, "metrics.log");
            cbdePerfLogFile = Path.Combine(cbdeProcessSpecificPath, "performances.log");
            cbdeDirectoryRoot = Path.Combine(cbdePath, "assemblies");
            Directory.CreateDirectory(cbdeDirectoryRoot);
        }

        private void ExportFunctionMlir(SyntaxTree tree, SemanticModel model, MlirExporterMetrics exporterMetrics, string mlirFileName)
        {
            using (var mlirStreamWriter = new StreamWriter(Path.Combine(cbdeDirectoryAssembly, mlirFileName)))
            {
                string perfLog = tree.GetRoot().GetLocation().GetLineSpan().Path + "\n";
                MLIRExporter mlirExporter = new MLIRExporter(mlirStreamWriter, model, exporterMetrics, true);
                foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var cpuWatch = ThreadCpuStopWatch.StartNew();
                    mlirExporter.ExportFunction(method);
                    perfLog += method.Identifier + " " + watch.ElapsedMilliseconds + "\n";
                    perfLog += method.Identifier + " " + cpuWatch.ElapsedMilliseconds + "\n";
                }
                PerformanceLog(perfLog + "\n");
            }
        }
        private void RunCbdeAndRaiseIssues(CompilationAnalysisContext c)
        {
            Log("Running CBDE");
            using (Process pProcess = new Process())
            {
                LogIfFailure("- Cbde process");
                pProcess.StartInfo.FileName = cbdeBinaryPath;
                pProcess.StartInfo.WorkingDirectory = cbdeDirectoryAssembly;
                var cbdePerfLogFile = Path.Combine(cbdeDirectoryAssembly, "perfLogFile.log");

                pProcess.StartInfo.Arguments = $"-i \"{cbdeDirectoryAssembly}\" -o \"{cbdeJsonOutputPath}\" -s \"{cbdePerfLogFile}\"";

                LogIfFailure($"  * binary_location: '{pProcess.StartInfo.FileName}'");
                LogIfFailure($"  * arguments: '{pProcess.StartInfo.Arguments}'");

                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardError = true;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.Start();
                long totalProcessorTime=0, peakPagedMemory = 0, peakWorkingSet=0;
                while(!pProcess.WaitForExit(processStatPeriodMs))
                {
                    try
                    {
                        pProcess.Refresh();
                        totalProcessorTime = (long)pProcess.TotalProcessorTime.TotalMilliseconds;
                        peakPagedMemory = pProcess.PeakPagedMemorySize64;
                        peakWorkingSet = pProcess.PeakWorkingSet64;
                    }
                    catch (InvalidOperationException) {
                        // the process might have exited during the loop
                    }
                }

                var logString = $@" *exit code: {pProcess.ExitCode}
  * cpu_time: {totalProcessorTime} ms
  * peak_paged_mem: {peakPagedMemory >> 20} MB
  * peak_working_set: {peakWorkingSet >> 20} MB";

                Log(logString);
                LogIfFailure(logString);

                if (pProcess.ExitCode != 0)
                {
                    Log("Running CBDE: Failure");
                    LogFailedCbdeRun(pProcess);
                    Log("Running CBDE: Error dumped");
                }
                else
                {
                    Log("Running CBDE: Success");
                    RaiseIssuesFromJSon(c);
                    Cleanup();
                    Log("Running CBDE: Issues reported");
                }
            }
        }
        private void Cleanup()
        {
            logStringBuilder.Clear();
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
            LogIfFailure($"- parsing json file {cbdeJsonOutputPath}");
            failureString.Append("  content of stderr is:\n" + pProcess.StandardError.ReadToEnd());
            failureString.Append("  content of the CBDE handler log file is :\n" + logStringBuilder.ToString());
            Log(failureString.ToString());
            Console.Error.WriteLine($"Error when executing CBDE, more details in {cbdeProcessSpecificPath}");
        }

        private void RaiseIssuesFromJSon(CompilationAnalysisContext context)
        {
            string jsonFileContent;
            List<List<JObject>> jsonIssues;
            LogIfFailure($"- parsing json file {cbdeJsonOutputPath}");
            try
            {
                jsonFileContent = File.ReadAllText(cbdeJsonOutputPath);
                jsonIssues = JsonConvert.DeserializeObject<List<List<JObject>>>(jsonFileContent);
            }
            catch
            {
                LogIfFailure($"- error parsing json file {cbdeJsonOutputPath}");
                Log(logStringBuilder.ToString());
                Console.Error.WriteLine($"Error parsing output from CBDE, more details in {cbdeProcessSpecificPath}");
                return;
            }

            foreach (var issue in jsonIssues.First())
            {
                LogIfFailure($"  * processing token {issue.ToString()}");
                try
                {
                    RaiseIssueFromJToken(issue, context);
                }
                catch
                {
                    LogIfFailure($"  * error reporting token {cbdeJsonOutputPath}");
                    Log(logStringBuilder.ToString());
                    Console.Error.WriteLine($"Error raising issue from CBDE, more details in {cbdeProcessSpecificPath}");
                    continue;
                }
            }
        }

        private class ThreadCpuStopWatch
        {
            private double totalMsStart;

            private readonly ProcessThread currentProcessThread;

            public long ElapsedMilliseconds =>
                (long)(currentProcessThread.TotalProcessorTime.TotalMilliseconds - totalMsStart);

            private static ProcessThread GetCurrentProcessThread()
            {
                var currentId = AppDomain.GetCurrentThreadId();
                // this is not a generic collection, so there is no linq way of doing that
                foreach (ProcessThread p in Process.GetCurrentProcess().Threads)
                {
                    if (p.Id == currentId)
                    {
                        return p;
                    }
                }
                return null;
            }

            public void Reset()
            {
                totalMsStart = currentProcessThread.TotalProcessorTime.TotalMilliseconds;
            }

            private ThreadCpuStopWatch()
            {
                currentProcessThread = GetCurrentProcessThread();
            }

            // We are copying the interface of the class StopWatch
            public static ThreadCpuStopWatch StartNew()
            {
                var instance = new ThreadCpuStopWatch();
                instance.Reset();
                return instance;
            }
        }
    }
}
