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

using System;
using SonarAnalyzer.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using System.Collections.Immutable;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(S2583DiagnosticId)]
    [Rule(S3949DiagnosticId)]
    public sealed class CbdeHandlerRule : SonarDiagnosticAnalyzer
    {
        CbdeHandler cbdeHandler = new CbdeHandler();
        private const string S2583DiagnosticId = "S2583";
        private const string S2583MessageFormat = "{0}";
        private static readonly DiagnosticDescriptor ruleS2583 = DiagnosticDescriptorBuilder.GetDescriptor(S2583DiagnosticId, S2583MessageFormat, RspecStrings.ResourceManager, fadeOutCode: true);

        private const string S3949DiagnosticId = "S3949";
        private const string S3949MessageFormat = "{0}";
        private static readonly DiagnosticDescriptor ruleS3949 = DiagnosticDescriptorBuilder.GetDescriptor(S3949DiagnosticId, S3949MessageFormat, RspecStrings.ResourceManager, fadeOutCode: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(ruleS2583, ruleS3949);

        private ImmutableDictionary<string, DiagnosticDescriptor> ruleIdToDiagDescriptor = ImmutableDictionary<string, DiagnosticDescriptor>.Empty
            .Add("S2583", ruleS2583)
            .Add("S3949", ruleS3949);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            cbdeHandler.Initialize(context, OnCbdeIssue);
        }

        private void OnCbdeIssue(String key, String message, Location loc, CompilationAnalysisContext context)
        {
            if (!ruleIdToDiagDescriptor.ContainsKey(key))
                throw new InvalidOperationException($"CBDE should not raise issues on key {key}");

            context.ReportDiagnosticWhenActive(Diagnostic.Create(ruleIdToDiagDescriptor[key], loc, message));
        }
    }
}
