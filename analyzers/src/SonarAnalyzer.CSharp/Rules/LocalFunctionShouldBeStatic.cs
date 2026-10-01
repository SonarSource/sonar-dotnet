/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LocalFunctionShouldBeStatic : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S9415";
    private const string MessageFormat = "Make this local function static.";
    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    // Mirrors Roslyn's IDE0062 ("Make local function static"):
    // https://github.com/dotnet/roslyn/blob/00ba50192fea355a7d8521c05e015f3cf9a1059a/src/Analyzers/CSharp/Analyzers/MakeLocalFunctionStatic/MakeLocalFunctionStaticDiagnosticAnalyzer.cs
    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            context =>
            {
                var localFunction = (LocalFunctionStatementSyntaxWrapper)context.Node;
                if (context.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp8)
                    && !localFunction.Modifiers.Any(SyntaxKind.StaticKeyword)
                    && localFunction is not { Body: null, ExpressionBody: null }
                    && IsIndependent(localFunction, context.Model, [], context.Cancel))
                {
                    context.ReportIssue(Rule, localFunction.Identifier);
                }
            },
            SyntaxKindEx.LocalFunctionStatement);

    // Returns true when neither 'localFunction' nor the non-static local functions it references capture state from outside of it.
    private static bool IsIndependent(LocalFunctionStatementSyntaxWrapper localFunction,
                                      SemanticModel model,
                                      HashSet<LocalFunctionStatementSyntaxWrapper> visited,
                                      CancellationToken cancel) =>
        !visited.Add(localFunction)
        // Variables of 'localFunction' itself can be captured by its nested lambdas and local functions
        || (model.AnalyzeDataFlow(localFunction) is { Succeeded: true } dataFlow
            && dataFlow.CapturedInside.All(x => IsDeclaredWithin(x, localFunction))
            && dataFlow.UsedLocalFunctions.All(x => IsIndependentUsage(x, localFunction, model, visited, cancel)));

    // Calling or referencing a non-static local function captures whatever that function captures.
    private static bool IsIndependent(IMethodSymbol usedLocalFunction, SemanticModel model, HashSet<LocalFunctionStatementSyntaxWrapper> visited, CancellationToken cancel) =>
        usedLocalFunction.DeclaringSyntaxReferences.FirstOrDefault() is { } reference
        && reference.SyntaxTree == model.SyntaxTree
        && IsIndependent((LocalFunctionStatementSyntaxWrapper)reference.GetSyntax(cancel), model, visited, cancel);

    private static bool IsIndependentUsage(IMethodSymbol usedLocalFunction,
                                           LocalFunctionStatementSyntaxWrapper localFunction,
                                           SemanticModel model,
                                           HashSet<LocalFunctionStatementSyntaxWrapper> visited,
                                           CancellationToken cancel) =>
        usedLocalFunction.IsStatic
        || IsDeclaredWithin(usedLocalFunction, localFunction)
        || !IsReferencedWithin(usedLocalFunction, localFunction, model, cancel) // UsedLocalFunctions is not limited to the analyzed region
        || IsIndependent(usedLocalFunction, model, visited, cancel);

    private static bool IsReferencedWithin(IMethodSymbol usedLocalFunction, SyntaxNode node, SemanticModel model, CancellationToken cancel) =>
        node.DescendantNodes()
            .OfType<SimpleNameSyntax>()
            .Any(x => x.Identifier.ValueText == usedLocalFunction.Name
                        && usedLocalFunction.OriginalDefinition.Equals(model.GetSymbolInfo(x, cancel).Symbol?.OriginalDefinition));

    private static bool IsDeclaredWithin(ISymbol symbol, SyntaxNode node) =>
        symbol.DeclaringSyntaxReferences.Any(x => x.SyntaxTree == node.SyntaxTree && node.Span.Contains(x.Span));
}
