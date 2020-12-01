/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Runtime.CompilerServices;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.CBDE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using System.Collections.Immutable;
using System.Linq;
using System.IO;
using System.Diagnostics.CodeAnalysis;

[assembly: InternalsVisibleTo("SonarAnalyzer.UnitTest" + Signing.InternalsVisibleToPublicKey)]

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(S3949DiagnosticId)]
    // Note: For now, this rule actually runs only under windows and outside of SonarLint
    public sealed class CbdeHandlerRule : SonarDiagnosticAnalyzer
    {
        private readonly bool unitTest;

        private const string S3949DiagnosticId = "S3949";
        private const string S3949MessageFormat = "{0}";
        private static readonly DiagnosticDescriptor ruleS3949 = DiagnosticDescriptorBuilder.GetDescriptor(S3949DiagnosticId, S3949MessageFormat, RspecStrings.ResourceManager, fadeOutCode: true);
        private static string WorkDirectoryBasePath;
        private readonly Action<string> onCbdeExecution;
        private readonly string testCbdeBinaryPath;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(ruleS3949);

        private readonly ImmutableDictionary<string, DiagnosticDescriptor> ruleIdToDiagDescriptor = ImmutableDictionary<string, DiagnosticDescriptor>.Empty
            .Add("S3949", ruleS3949);

        public CbdeHandlerRule() : this(false, null, null) {}

        private CbdeHandlerRule(bool unitTest, string testCbdeBinaryPath, Action<string> onCbdeExecution)
        {
            this.unitTest = unitTest;
            this.testCbdeBinaryPath = testCbdeBinaryPath;
            this.onCbdeExecution = onCbdeExecution;
        }

        internal static CbdeHandlerRule MakeUnitTestInstance(string testCbdeBinaryPath, Action<string> onCbdeExecution)
        {
            return new CbdeHandlerRule(true, testCbdeBinaryPath, onCbdeExecution);
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            // The available platform ids are documented here: https://docs.microsoft.com/en-us/dotnet/api/system.platformid?view=netframework-4.8
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                new CbdeHandler(context, OnCbdeIssue, ShouldRunCbdeInContext, () => WorkDirectoryBasePath,
                    testCbdeBinaryPath, onCbdeExecution);
            }
        }

        // This functions is called for each issue found by cbde after it runs
        private void OnCbdeIssue(String key, String message, Location loc, CompilationAnalysisContext context)
        {
            if (!ruleIdToDiagDescriptor.ContainsKey(key))
                throw new InvalidOperationException($"CBDE should not raise issues on key {key}");

            context.ReportDiagnosticWhenActive(Diagnostic.Create(ruleIdToDiagDescriptor[key], loc, message));
        }

        internal const string ConfigurationAdditionalFile = "ProjectOutFolderPath.txt";

        [ExcludeFromCodeCoverage]
        internal static bool IsProjectOutput(AdditionalText file) =>
           ParameterLoader.ConfigurationFilePathMatchesExpected(file.Path, ConfigurationAdditionalFile);

        // The logic used here is based on detecting a file which is present only when not run from SonarLint
        /// The logic is copied from <see cref="SonarAnalyzer.Rules.UtilityAnalyzerBase"/>
        [ExcludeFromCodeCoverage]
        internal static bool CalledFromSonarLint(CompilationAnalysisContext context)
        {
            var options = context.Options;
            var settings = PropertiesHelper.GetSettings(options);
            var projectOutputAdditionalFile = options.AdditionalFiles
                .FirstOrDefault(IsProjectOutput);

            if (!settings.Any() || projectOutputAdditionalFile == null)
            {
                return true;
            }
            WorkDirectoryBasePath = File.ReadAllLines(projectOutputAdditionalFile.Path).FirstOrDefault(l => !string.IsNullOrEmpty(l));

            return string.IsNullOrEmpty(WorkDirectoryBasePath);
        }

        // This function is called from CbdeHandler to check if it should run
        private bool ShouldRunCbdeInContext(CompilationAnalysisContext context)
        {
            return !CalledFromSonarLint(context) || unitTest;
        }
    }
}
