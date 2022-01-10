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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    /// <summary>
    /// Immutable builder that holds all parameters for rule verification.
    /// </summary>
    internal class VerifierBuilder
    {
        // All properties are (and should be) immutable.
        public ImmutableArray<Func<DiagnosticAnalyzer>> Analyzers { get; init; } = ImmutableArray<Func<DiagnosticAnalyzer>>.Empty;
        public CompilationErrorBehavior ErrorBehavior { get; init; } = CompilationErrorBehavior.Default;
        public ImmutableArray<DiagnosticDescriptor> OnlyDiagnostics { get; init; } = ImmutableArray<DiagnosticDescriptor>.Empty;
        public OutputKind OutputKind { get; init; } = OutputKind.DynamicallyLinkedLibrary;
        public ImmutableArray<string> Paths { get; init; } = ImmutableArray<string>.Empty;
        public ImmutableArray<ParseOptions> ParseOptions { get; init; } = ImmutableArray<ParseOptions>.Empty;
        public ImmutableArray<MetadataReference> References { get; init; } = ImmutableArray<MetadataReference>.Empty;
        public string SonarProjectConfigPath { get; init; }

        public VerifierBuilder() { }

        private VerifierBuilder(VerifierBuilder original)
        {
            Analyzers = original.Analyzers;
            ErrorBehavior = original.ErrorBehavior;
            OnlyDiagnostics = original.OnlyDiagnostics;
            OutputKind = original.OutputKind;
            Paths = original.Paths;
            ParseOptions = original.ParseOptions;
            References = original.References;
            SonarProjectConfigPath = original.SonarProjectConfigPath;
        }

        /// <summary>
        /// This method solves complicated scenarios. Use 'new VerifierBuilder&lt;TAnalyzer&gt;()' for single analyzer cases with no rule parameters.
        /// </summary>
        public VerifierBuilder AddAnalyzer(Func<DiagnosticAnalyzer> createConfiguredAnalyzer) =>
            new(this) { Analyzers = Analyzers.Append(createConfiguredAnalyzer).ToImmutableArray() };

        public VerifierBuilder AddPaths(params string[] paths) =>
            new(this) { Paths = Paths.Concat(paths).ToImmutableArray() };

        public VerifierBuilder AddReferences(IEnumerable<MetadataReference> references) =>
            references?.ToArray() is { Length: not 0 } referenceArray
                ? new(this) { References = References.Concat(referenceArray).ToImmutableArray() }
                : this;

        public VerifierBuilder WithErrorBehavior(CompilationErrorBehavior errorBehavior) =>
            new(this) { ErrorBehavior = errorBehavior };

        public VerifierBuilder WithLanguageVersion(CS.LanguageVersion languageVersion) =>
            WithOptions(ImmutableArray.Create<ParseOptions>(new CS.CSharpParseOptions(languageVersion)));

        public VerifierBuilder WithLanguageVersion(VB.LanguageVersion languageVersion) =>
            WithOptions(ImmutableArray.Create<ParseOptions>(new VB.VisualBasicParseOptions(languageVersion)));

        public VerifierBuilder WithOnlyDiagnostics(params DiagnosticDescriptor[] onlyDiagnostics) =>
            new(this) { OnlyDiagnostics = onlyDiagnostics.ToImmutableArray() };

        public VerifierBuilder WithOptions(ImmutableArray<ParseOptions> parseOptions) =>
            new(this) { ParseOptions = parseOptions };

        public VerifierBuilder WithOutputKind(OutputKind outputKind) =>
            new(this) { OutputKind = outputKind };

        public VerifierBuilder WithSonarProjectConfigPath(string sonarProjectConfigPath) =>
            new(this) { SonarProjectConfigPath = sonarProjectConfigPath };

        public VerifierBuilder WithTopLevelStatements()
        {
            if (ParseOptions.OfType<VB.VisualBasicParseOptions>().Any())
            {
                throw new InvalidOperationException($"{nameof(WithTopLevelStatements)} is not supported with {nameof(VB.VisualBasicParseOptions)}.");
            }
            if (ParseOptions.Cast<CS.CSharpParseOptions>().Any(x => x.LanguageVersion < CS.LanguageVersion.CSharp9))
            {
                throw new InvalidOperationException($"{nameof(WithTopLevelStatements)} is supported from {nameof(CS.LanguageVersion.CSharp9)}.");
            }
            return (ParseOptions.IsEmpty ? WithOptions(ParseOptionsHelper.FromCSharp9) : this)
                .WithOutputKind(OutputKind.ConsoleApplication);
        }

        public Verifier Build() =>
            new(this);
    }

    internal class VerifierBuilder<TAnalyzer> : VerifierBuilder
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public VerifierBuilder() =>
            Analyzers = new Func<DiagnosticAnalyzer>[] { () => new TAnalyzer() }.ToImmutableArray();
    }

    internal static class VerifierBuilderExtensions
    {
        public static void Verify(this VerifierBuilder builder) =>
            builder.Build().Verify();
    }
}
