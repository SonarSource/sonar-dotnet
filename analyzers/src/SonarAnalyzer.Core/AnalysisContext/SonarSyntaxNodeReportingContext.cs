/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.AnalysisContext;

public sealed class SonarSyntaxNodeReportingContext : SonarTreeReportingContextBase<SyntaxNodeAnalysisContext>
{
    public override SyntaxTree Tree => Context.Node.SyntaxTree;
    public override Compilation Compilation => Context.Compilation;
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;
    public SyntaxNode Node => Context.Node;
    public SemanticModel SemanticModel => Context.SemanticModel;
    public ISymbol ContainingSymbol => Context.ContainingSymbol;

    internal SonarSyntaxNodeReportingContext(SonarAnalysisContext analysisContext, SyntaxNodeAnalysisContext context) : base(analysisContext, context) { }

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

    private protected override ReportingContext CreateReportingContext(Diagnostic diagnostic) =>
        new(this, diagnostic);
}
