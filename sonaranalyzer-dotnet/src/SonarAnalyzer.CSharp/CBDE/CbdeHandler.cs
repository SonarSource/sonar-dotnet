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

namespace SonarAnalyzer.CBDE
{
    public class CbdeHandler
    {
        private const int processStatPeriodMs = 1000;
        private const string cbdeJsonOutputFileName = "cbdeSEout.json";

        private static bool initialized = false;
        // this is the place where the cbde executable is unpacked. It is in a temp folder
        private static string cbdeBinaryPath;
        private static readonly object logFileLock = new Object();
        private static readonly object metricsFileLock = new Object();
        private static readonly object perfFileLock = new Object();
        private static readonly object staticInitLock = new Object();

        private readonly Action<String, String, Location, CompilationAnalysisContext> raiseIssue;
        private readonly Func<CompilationAnalysisContext, bool> shouldRunInContext;
        private readonly Func<string> getOutputDirectory;

        protected HashSet<string> csSourceFileNames= new HashSet<string>();
        protected Dictionary<string, int> fileNameDuplicateNumbering = new Dictionary<string, int>();

        private StringBuilder logStringBuilder;

        // cbdePath is inside .sonarqube/out/<n>/
        // cbdeDirectoryRoot contains mlir files and results for each assembly
        // cbdeProcessSpecificPath is the $"{cbdePath}/CBDE_{pid}/" folder
        // cbdeLogFile, cbdeMetricsLogFile and cbdePerfLogFile are inside cbdeProcessSpecificPath
        private string cbdePath;
        private string cbdeDirectoryRoot;
        private string cbdeDirectoryAssembly;
        private string cbdeJsonOutputPath;
        private string cbdeLogFile;
        private string cbdeMetricsLogFile;
        private string cbdePerfLogFile;
        private string moreDetailsMessage;
        private bool emitLog = false;

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

        private void LogIfFailure(string s)
        {
            if (emitLog)
            {
                logStringBuilder.AppendLine(s);
            }
        }

        private void Log(string s)
        {
            if (emitLog)
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
                        var message = $@"Top level error in CBDE handling: {e.Message}
Details: {this.moreDetailsMessage}
Inner exception: {e.InnerException}
Stack trace: {e.StackTrace}";
                        // Roslyn/MSBuild is currently cutting exception message at the end of the line instead
                        // of displaying the full message. As a workaround, we replace the line ending with ' ## '.
                        // See https://github.com/dotnet/roslyn/issues/1455 and https://github.com/dotnet/roslyn/issues/24346
                        throw new CbdeException(message.Replace("\n", " ## ").Replace("\r", ""));
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
            var cbdeProcessSpecificPath = Path.Combine(cbdePath, $"CBDE_{Process.GetCurrentProcess().Id}");
            Directory.CreateDirectory(cbdeProcessSpecificPath);
            cbdeLogFile = Path.Combine(cbdeProcessSpecificPath, "cbdeHandler.log");
            moreDetailsMessage = emitLog ? $", more details in {cbdeProcessSpecificPath}" : "";
            cbdeMetricsLogFile = Path.Combine(cbdeProcessSpecificPath, "metrics.log");
            cbdePerfLogFile = Path.Combine(cbdeProcessSpecificPath, "performances.log");
            cbdeDirectoryRoot = Path.Combine(cbdePath, "assemblies");
            Directory.CreateDirectory(cbdeDirectoryRoot);
        }

        private void ExportFunctionMlir(SyntaxTree tree, SemanticModel model, MlirExporterMetrics exporterMetrics, string mlirFileName)
        {
            using (var mlirStreamWriter = new StreamWriter(Path.Combine(cbdeDirectoryAssembly, mlirFileName)))
            {
                StringBuilder perfLog = new StringBuilder();
                perfLog.AppendLine(tree.GetRoot().GetLocation().GetLineSpan().Path);
                var mlirExporter = new MlirExporter(mlirStreamWriter, model, exporterMetrics, true);
                foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var cpuWatch = ThreadCpuStopWatch.StartNew();
                    mlirExporter.ExportFunction(method);
                    perfLog.AppendLine(method.Identifier + " " + watch.ElapsedMilliseconds);
                    perfLog.AppendLine(method.Identifier + " " + cpuWatch.ElapsedMilliseconds);
                }
                PerformanceLog(perfLog.ToString() + "\n");
            }
        }

        private void RunCbdeAndRaiseIssues(CompilationAnalysisContext c)
        {
            Log("Running CBDE");
            using (Process cbdeProcess = new Process())
            {
                LogIfFailure("- Cbde process");
                cbdeProcess.StartInfo.FileName = cbdeBinaryPath;
                cbdeProcess.StartInfo.WorkingDirectory = cbdeDirectoryAssembly;
                var cbdeExePerfLogFile = Path.Combine(cbdeDirectoryAssembly, "perfLogFile.log");

                cbdeProcess.StartInfo.Arguments = $"-i \"{cbdeDirectoryAssembly}\" -o \"{cbdeJsonOutputPath}\" -s \"{cbdeExePerfLogFile}\"";

                LogIfFailure($"  * binary_location: '{cbdeProcess.StartInfo.FileName}'");
                LogIfFailure($"  * arguments: '{cbdeProcess.StartInfo.Arguments}'");

                cbdeProcess.StartInfo.UseShellExecute = false;
                cbdeProcess.StartInfo.RedirectStandardError = true;
                long totalProcessorTime = 0;
                long peakPagedMemory = 0;
                long peakWorkingSet = 0;
                try
                {
                    cbdeProcess.Start();
                    while (!cbdeProcess.WaitForExit(processStatPeriodMs))
                    {
                        try
                        {
                            cbdeProcess.Refresh();
                            totalProcessorTime = (long)cbdeProcess.TotalProcessorTime.TotalMilliseconds;
                            peakPagedMemory = cbdeProcess.PeakPagedMemorySize64;
                            peakWorkingSet = cbdeProcess.PeakWorkingSet64;
                        }
                        catch (InvalidOperationException)
                        {
                            // the process might have exited during the loop
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new CbdeException($"Exception while running CBDE process: {e.Message}{this.moreDetailsMessage}");
                }

                var logString = $@" *exit code: {cbdeProcess.ExitCode}
  * cpu_time: {totalProcessorTime} ms
  * peak_paged_mem: {peakPagedMemory >> 20} MB
  * peak_working_set: {peakWorkingSet >> 20} MB";

                Log(logString);
                LogIfFailure(logString);

                if (cbdeProcess.ExitCode == 0)
                {
                    Log("Running CBDE: Success");
                    RaiseIssuesFromJSon(c);
                    Cleanup();
                    Log("Running CBDE: Issues reported");
                }
                else
                {
                    Log("Running CBDE: Failure");
                    LogFailedCbdeRun(cbdeProcess);
                    Log("Running CBDE: Error dumped");
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
            throw new CbdeException($"CBDE external process reported an error{this.moreDetailsMessage}");
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
            catch(Exception exception)
            {
                if (exception is JsonException || exception is IOException)
                {
                    LogIfFailure($"- error parsing json file {cbdeJsonOutputPath}: {exception.ToString()}");
                    Log(logStringBuilder.ToString());
                    throw new CbdeException($"Error parsing output from CBDE: {exception.Message}{this.moreDetailsMessage}");
                }
                throw;
            }

            // in cbde json output there is enclosing {}, so this is considered as a list of list
            // with one element in the outer list
            foreach (var issue in jsonIssues.First())
            {
                LogIfFailure($"  * processing token {issue.ToString()}");
                try
                {
                    RaiseIssueFromJToken(issue, context);
                }
                catch(Exception e)
                {
                    if (e is SystemException || e is JsonException)
                    {
                        LogIfFailure($"  * error reporting token {cbdeJsonOutputPath}: {e.ToString()}");
                        Log(logStringBuilder.ToString());
                        throw new CbdeException($"Error raising issue from CBDE: {e.Message}{this.moreDetailsMessage}");
                    }
                    throw;
                }
            }
        }

        private class ThreadCpuStopWatch
        {
            private double totalMsStart;

            private readonly ProcessThread currentProcessThread;

            public void Reset()
            {
                totalMsStart = currentProcessThread?.TotalProcessorTime.TotalMilliseconds ?? 0;
            }

            public long ElapsedMilliseconds =>
                (long)((currentProcessThread?.TotalProcessorTime.TotalMilliseconds ?? -1) - totalMsStart);

            private static ProcessThread GetCurrentProcessThread()
            {
                // we need the physical thread id to get the cpu time.
                // contrary to what the deprecation warning says, in this case,
                // it cannot be replaced with the ManagedThreadId property on Thread
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
