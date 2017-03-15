/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using System.IO;
using Google.Protobuf;
using SonarAnalyzer.Protobuf;
using System.Diagnostics;

namespace SonarAnalyzer.Runner
{
    public static class Program
    {
        internal const string IssuesFileName = "issues.pb";

        public static int RunAnalysis(ScannerAnalyzerConfiguration conf)
        {
            var language = AnalyzerLanguage.Parse(conf.Language);

            Write($"SonarAnalyzer for {language.GetFriendlyName()} version {FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion}");

            var configuration = new Configuration(conf.InputConfigurationPath, conf.WorkDirectoryConfigFilePath, language);
            var diagnosticsRunner = new DiagnosticsRunner(configuration);

            var outputDirectory = conf.OutputFolderPath;
            Directory.CreateDirectory(outputDirectory);

            var currentFileIndex = 0;

            using (var tokentypeStream = File.Create(Path.Combine(outputDirectory, Rules.TokenTypeAnalyzerBase.TokenTypeFileName)))
            using (var symRefStream = File.Create(Path.Combine(outputDirectory, Rules.SymbolReferenceAnalyzerBase.SymbolReferenceFileName)))
            using (var cpdStream = File.Create(Path.Combine(outputDirectory, Rules.CopyPasteTokenAnalyzerBase.CopyPasteTokenFileName)))
            using (var metricsStream = File.Create(Path.Combine(outputDirectory, Rules.MetricsAnalyzerBase.MetricsFileName)))
            using (var encodingStream = File.Create(Path.Combine(outputDirectory, Rules.FileEncodingAnalyzerBase.EncodingFileName)))
            using (var issuesStream = File.Create(Path.Combine(outputDirectory, IssuesFileName)))
            {
                foreach (var file in configuration.Files)
                {
                    #region Single file processing

                    Write(currentFileIndex + "/" + configuration.Files.Count + " files analyzed, starting to analyze: " + file);
                    currentFileIndex++;

                    try
                    {
                        var solution = CompilationHelper.GetSolutionFromFiles(file, configuration.Encoding, language);

                        var compilation = solution.Projects.First().GetCompilationAsync().Result;
                        var syntaxTree = compilation.SyntaxTrees.First();

                        var tokenCollector = new TokenCollector(file, solution.GetDocument(syntaxTree));

                        tokenCollector.TokenTypeInfo.WriteDelimitedTo(tokentypeStream);
                        tokenCollector.SymbolReferenceInfo.WriteDelimitedTo(symRefStream);
                        tokenCollector.CopyPasteTokenInfo.WriteDelimitedTo(cpdStream);

                        MetricsBase metrics;
                        Rules.FileEncodingAnalyzerBase encodingCalculator;

                        if (language == AnalyzerLanguage.CSharp)
                        {
                            metrics = new Common.CSharp.Metrics(syntaxTree);
                            encodingCalculator = new Rules.CSharp.FileEncodingAnalyzer();
                        }
                        else
                        {
                            metrics = new Common.VisualBasic.Metrics(syntaxTree);
                            encodingCalculator = new Rules.VisualBasic.FileEncodingAnalyzer();
                        }

                        var metricsInfo = Rules.MetricsAnalyzerBase.CalculateMetrics(metrics, file, configuration.IgnoreHeaderComments);
                        metricsInfo.WriteDelimitedTo(metricsStream);

                        var encodingInfo = Rules.FileEncodingAnalyzerBase.CalculateEncoding(encodingCalculator, syntaxTree, file);
                        encodingInfo.WriteDelimitedTo(encodingStream);

                        var issuesInFile = new FileIssues
                        {
                            FilePath = file
                        };

                        foreach (var diagnostic in diagnosticsRunner.GetDiagnostics(compilation))
                        {
                            var issue = new FileIssues.Types.Issue
                            {
                                Id = diagnostic.Id,
                                Message = diagnostic.GetMessage()
                            };

                            if (diagnostic.Location != Location.None)
                            {
                                issue.Location = Rules.UtilityAnalyzerBase.GetTextRange(diagnostic.Location.GetLineSpan());
                            }

                            issuesInFile.Issue.Add(issue);
                        }

                        if (issuesInFile.Issue.Any())
                        {
                            issuesInFile.WriteDelimitedTo(issuesStream);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("Failed to analyze the file: " + file);
                        Console.Error.WriteLine(e);
                        return 1;
                    }

                    #endregion
                }
            }

            return 0;
        }

        public static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Write("Expected parameters: ");
                Write("[Input configuration path]");
                Write("[Output folder path]");
                Write("[AnalyzerLanguage: 'cs' for C#, 'vbnet' for VB.Net]");

                return -1;
            }

            return RunAnalysis(new ScannerAnalyzerConfiguration
            {
                InputConfigurationPath = args[0],
                OutputFolderPath = args[1],
                Language = args[2]
            });
        }

        internal static void Write(string text)
        {
            Console.WriteLine(text);
        }
    }
}
