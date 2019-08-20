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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using SonarAnalyzer.Common;
using System.Collections.Generic;

namespace SonarAnalyzer.Rules.CSharp
{
    public abstract class CbdeHandler : SonarDiagnosticAnalyzer
    {
        private const string cbdeSymbExecBinaryPath = "c:\\Temp\\dotnet-symbolic-execution.exe";
        private const string mlirDirectory = "c:\\Temp\\";
        private const string cbdeJsonOutputFilePath = mlirDirectory + "\\cbdeSEout.json";

        internal const string DiagnosticId = "S2583";
        protected const string MessageFormat = "Condition is always {0}";

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            RegisterMlirGeneration(context);
            Console.WriteLine("Hello Debug");
            RegisterCbseSymbolicExecution(context);
        }

        private void RegisterMlirGeneration(SonarAnalysisContext context)
        {
            context.RegisterCodeBlockStartAction<SyntaxKind>(
            c =>
                {
                    var firstFdecl = c.GetSyntaxTree().GetRoot().DescendantNodes().First();
                    var mlirFileName = firstFdecl.SyntaxTree.FilePath;
                    mlirFileName.Replace(":", "_COLON_");
                    mlirFileName.Replace("/", "_SLASH_");
                    mlirFileName.Replace("\\", "_BSLASH_");
                    mlirFileName += ".mlir";

                    using (StreamWriter streamWriter = new StreamWriter(mlirDirectory + mlirFileName))
                    {
                        MLIRExporter mlirExporter = new MLIRExporter(streamWriter, c.SemanticModel, true);
                        foreach (var method in c.GetSyntaxTree().GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                        {
                            mlirExporter.ExportFunction(method);
                        }
                    }
                });
        }
        private void RegisterCbseSymbolicExecution(SonarAnalysisContext context)
        {
            context.RegisterCompilationAction(
                c =>
                {
                    using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
                    {
                        pProcess.StartInfo.FileName = cbdeSymbExecBinaryPath;
                        pProcess.StartInfo.Arguments = "-i "+mlirDirectory + " -o " + cbdeJsonOutputFilePath;
                        pProcess.StartInfo.UseShellExecute = false;
                        pProcess.StartInfo.RedirectStandardOutput = true;
                        pProcess.StartInfo.RedirectStandardError = true;
                        pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        pProcess.StartInfo.CreateNoWindow = true;
                        pProcess.Start();
                        string output = pProcess.StandardOutput.ReadToEnd();
                        string eoutput = pProcess.StandardError.ReadToEnd();
                        //var mem = pProcess.PeakWorkingSet64;
                        pProcess.WaitForExit();
                        Console.WriteLine(
                            "Process finished with exit code {0}",
                             pProcess.ExitCode);
                        Console.WriteLine(
                            "Standard output is\n{0}",
                            output);
                        Console.WriteLine(
                            "Standard error is\n{0}",
                            eoutput);
                        // TODO: log this pProcess.PeakWorkingSet64;
                        if (pProcess.ExitCode != 0)
                        {
                            // log output
                            // do it again enabling SonarCBDE logging
                        }
                        else
                        {
                            RaiseIssuesFromJSon(c);
                        }
                    }

                }
                );
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
            try
            {
                jsonFileContent = File.ReadAllText(cbdeJsonOutputFilePath);
                jsonIssues = JsonConvert.DeserializeObject< List<List<JObject>>>(jsonFileContent);
            }
            catch
            {
                Console.WriteLine("Error parsing json file {0}", cbdeJsonOutputFilePath);
                return;
            }

            foreach (var issue in jsonIssues.First())
            {
                Console.WriteLine("Processing token {0}", issue.ToString());
                try
                {
                    context.ReportDiagnosticWhenActive(CreateDiagnosticFromJToken(issue));
                }
                catch
                {
                    Console.WriteLine("Error reporting token {0}", cbdeJsonOutputFilePath);
                    continue;
                }
            }

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
