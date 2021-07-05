/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.CBDE;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(S3949DiagnosticId)]
    // Note (1): For now, this rule actually runs only under windows and outside of SonarLint
    // Note (2): This rule is disabled for integration tests due to inconsistent behavior.
    // After removing the CBDE and rewriting the rule please don't forget to enable it in `AllSonarAnalyzerRules.ruleset`
    public sealed class CbdeHandlerRule : SonarDiagnosticAnalyzer
    {
        private const string S3949DiagnosticId = "S3949";
        private const string S3949MessageFormat = "{0}";
        private static readonly DiagnosticDescriptor RuleS3949 = DiagnosticDescriptorBuilder.GetDescriptor(S3949DiagnosticId, S3949MessageFormat, RspecStrings.ResourceManager, fadeOutCode: true);
        private readonly ImmutableDictionary<string, DiagnosticDescriptor> ruleIdToDiagDescriptor = ImmutableDictionary<string, DiagnosticDescriptor>.Empty.Add("S3949", RuleS3949);
        private readonly bool unitTest;
        private readonly Action<string> onCbdeExecution;
        private readonly string testCbdeBinaryPath;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(RuleS3949);

        public CbdeHandlerRule() : this(false, null, null) { }

        private CbdeHandlerRule(bool unitTest, string testCbdeBinaryPath, Action<string> onCbdeExecution)
        {
            this.unitTest = unitTest;
            this.testCbdeBinaryPath = testCbdeBinaryPath;
            this.onCbdeExecution = onCbdeExecution;
        }

        internal static CbdeHandlerRule MakeUnitTestInstance(string testCbdeBinaryPath, Action<string> onCbdeExecution) =>
            new CbdeHandlerRule(true, testCbdeBinaryPath, onCbdeExecution);

        protected override void Initialize(SonarAnalysisContext context)
        {
            // The available platform ids are documented here: https://docs.microsoft.com/en-us/dotnet/api/system.platformid?view=netframework-4.8
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var handler = new CbdeHandler(OnCbdeIssue, unitTest, testCbdeBinaryPath, onCbdeExecution);
                handler.RegisterMlirAndCbdeInOneStep(context);
            }
        }

        // This functions is called for each issue found by cbde after it runs
        private void OnCbdeIssue(string key, string message, Location loc, CompilationAnalysisContext context)
        {
            if (!ruleIdToDiagDescriptor.ContainsKey(key))
            {
                throw new InvalidOperationException($"CBDE should not raise issues on key {key}");
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(ruleIdToDiagDescriptor[key], loc, message));
        }
    }
}
