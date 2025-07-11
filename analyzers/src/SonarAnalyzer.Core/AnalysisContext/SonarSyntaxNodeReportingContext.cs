﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.AnalysisContext;

// Performance note: this struct is used in the Roslyn analysis context and should be as lightweight as possible. It should not be boxed on the hot-path in the
// registration (e.g. in SonarCompilationStartAnalysisContext.RegisterNodeAction) which is called for all matching syntax kinds of all syntax trees.
// It is okay to box during issue reporting because reporting is rare in comparission.
public readonly record struct SonarSyntaxNodeReportingContext(SonarAnalysisContext AnalysisContext, SyntaxNodeAnalysisContext Context) : ITreeReport, IAnalysisContext
{
    public SyntaxTree Tree => Context.Node.SyntaxTree;
    public Compilation Compilation => Context.Compilation;
    public AnalyzerOptions Options => Context.Options;
    public CancellationToken Cancel => Context.CancellationToken;
    public SyntaxNode Node => Context.Node;
    public SemanticModel Model => Context.SemanticModel;
    public ISymbol ContainingSymbol => Context.ContainingSymbol;

    /// <summary>
    /// Roslyn invokes the analyzer twice for positional records. The first invocation is for the class declaration and the second for the ctor represented by the positional parameter list.
    /// This behavior has been fixed since the Roslyn version 4.2.0 but we still need this for the proper support of Roslyn 4.0.0.
    /// </summary>
    /// <returns>
    /// Returns <see langword="true"/> for the invocation on the class declaration and <see langword="false"/> for the ctor invocation.
    /// </returns>
    /// <example>
    /// record R(int i);
    /// </example>
    /// <seealso href="https://github.com/dotnet/roslyn/issues/53136"/>
    public bool IsRedundantPositionalRecordContext() =>
        Context.ContainingSymbol.Kind == SymbolKind.Method;

    /// <summary>
    /// Roslyn invokes the analyzer twice for PrimaryConstructorBaseType. The ContainingSymbol is first the type and second the constructor. This check filters can be used to filter
    /// the first invocation. See also <seealso href="https://github.com/dotnet/roslyn/issues/70488">#Roslyn/70488</seealso>.
    /// </summary>
    /// <returns>
    /// Returns <see langword="true"/> for the invocation with PrimaryConstructorBaseType and ContainingSymbol being <see cref="SymbolKind.NamedType"/> and
    /// <see langword="false"/> for everything else.
    /// </returns>
    public bool IsRedundantPrimaryConstructorBaseTypeContext() =>
        Context is
        {
            Node.RawKind: (int)SyntaxKindEx.PrimaryConstructorBaseType,
            Compilation.Language: LanguageNames.CSharp,
            ContainingSymbol.Kind: SymbolKind.NamedType,
        };

    public bool IsAzureFunction() =>
        AzureFunctionMethod() is not null;

    public IMethodSymbol AzureFunctionMethod() =>
        Context.ContainingSymbol is IMethodSymbol method && method.HasAttribute(KnownType.Microsoft_Azure_WebJobs_FunctionNameAttribute)
            ? method
            : null;

    public bool IsRazorAnalysisEnabled() =>
        AnalysisContext.IsRazorAnalysisEnabled(Options, Compilation);

    public ReportingContext CreateReportingContext(Diagnostic diagnostic) =>
        new(this, diagnostic);

    public void ReportIssue(DiagnosticDescriptor rule,
                            Location primaryLocation,
                            IEnumerable<SecondaryLocation> secondaryLocations = null,
                            ImmutableDictionary<string, string> properties = null,
                            params string[] messageArgs)
    {
        var @this = this; // This boxes this struct in the capture below, but that is okay because reporting is rare and not on the hot-path.
        IssueReporter.ReportIssueCore(
            Compilation,
            x => @this.HasMatchingScope(x),
            CreateReportingContext,
            rule,
            primaryLocation,
            secondaryLocations,
            properties,
            messageArgs);
    }

    [Obsolete("Use another overload of ReportIssue, without calling Diagnostic.Create")]
    public void ReportIssue(Diagnostic diagnostic)
    {
        var @this = this;
        IssueReporter.ReportIssueCore(
            x => @this.HasMatchingScope(x),
            CreateReportingContext,
            diagnostic);
    }
}
