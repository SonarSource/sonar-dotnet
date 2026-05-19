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

using NodeSymbolAndModel = SonarAnalyzer.Core.Common.NodeSymbolAndModel<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax, Microsoft.CodeAnalysis.IMethodSymbol>;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnusedReturnValue : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3241";
    private const string MessageFormat = "Change return type to 'void'; not a single caller uses the returned value.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterSymbolAction(AnalyzeNamedTypes, SymbolKind.NamedType);
        context.RegisterNodeAction(AnalyzeLocalFunctionStatements, SyntaxKindEx.LocalFunctionStatement);
    }

    private static void AnalyzeNamedTypes(SonarSymbolReportingContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (!namedType.IsClassOrStruct() || namedType.ContainingType is not null)
        {
            return;
        }

        var removableDeclarationCollector = new CSharpRemovableDeclarationCollector(namedType, context.Compilation);

        var declaredPrivateMethodsWithReturn = CollectRemovableMethods(removableDeclarationCollector).ToList();
        if (!declaredPrivateMethodsWithReturn.Any())
        {
            return;
        }

        var invocations = removableDeclarationCollector.TypeDeclarations.SelectMany(FilterInvocations).ToList();

        foreach (var declaredPrivateMethodWithReturn in declaredPrivateMethodsWithReturn)
        {
            var matchingInvocations = invocations
                .Where(x => MatchesDeclaration(x.Symbol, declaredPrivateMethodWithReturn.Symbol))
                .ToList();

            // Method invocation is noncompliant when there is at least 1 invocation of the method, and no invocation is using the return value. The case of 0 invocation is handled by S1144.
            if (matchingInvocations.Any() && !matchingInvocations.Any(IsReturnValueUsed))
            {
                context.ReportIssue(Rule, declaredPrivateMethodWithReturn.Node.ReturnType);
            }
        }
    }

    private static void AnalyzeLocalFunctionStatements(SonarSyntaxNodeReportingContext context)
    {
        var localFunctionSyntax = (LocalFunctionStatementSyntaxWrapper)context.Node;
        var topMostContainingMethod = localFunctionSyntax.IsTopLevel()
            ? context.Node.Parent.Parent // .Parent.Parent is the CompilationUnit
            : context.Node.GetTopMostContainingMethod();

        if (topMostContainingMethod is null)
        {
            return;
        }

        var localFunctionSymbol = (IMethodSymbol)context.Model.GetDeclaredSymbol(localFunctionSyntax);
        if (localFunctionSymbol.ReturnsVoid || localFunctionSymbol.IsAsync)
        {
            return;
        }

        var matchingInvocations = LocalMatchingInvocations(topMostContainingMethod, localFunctionSymbol, context.Model).ToList();
        // Method invocation is noncompliant when there is at least 1 invocation of the method, and no invocation is using the return value. The case of 0 invocation is handled by S1144.
        if (matchingInvocations.Any() && !matchingInvocations.Any(IsReturnValueUsed))
        {
            context.ReportIssue(Rule, localFunctionSyntax.ReturnType);
        }
    }

    private static bool IsReturnValueUsed(NodeSymbolAndModel matchingInvocation) =>
        !IsExpressionStatement(matchingInvocation.Node.Parent)
        && !IsActionLambda(matchingInvocation.Node.Parent, matchingInvocation.Model);

    private static bool IsActionLambda(SyntaxNode node, SemanticModel model) =>
        node is LambdaExpressionSyntax lambda
        && model.GetSymbolInfo(lambda).Symbol is IMethodSymbol { ReturnsVoid: true };

    private static bool IsExpressionStatement(SyntaxNode node) =>
        node is ExpressionStatementSyntax;

    private static IEnumerable<NodeSymbolAndModel> LocalMatchingInvocations(SyntaxNode containingMethod, IMethodSymbol invocationSymbol, SemanticModel semanticModel) =>
        containingMethod.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(x => semanticModel.GetSymbolInfo(x.Expression).Symbol is IMethodSymbol methodSymbol && invocationSymbol.Equals(methodSymbol))
            .Select(x => new NodeSymbolAndModel(x, invocationSymbol, semanticModel))
            .ToList();

    private static IEnumerable<NodeSymbolAndModel> FilterInvocations(NodeAndModel<BaseTypeDeclarationSyntax> container) =>
        container.Node.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(x => new NodeSymbolAndModel(x, Unreduce(container.Model.GetSymbolInfo(x).Symbol as IMethodSymbol), container.Model))
            .Where(x => x.Symbol is not null);

    private static IEnumerable<NodeSymbolAndModel<MethodDeclarationSyntax, IMethodSymbol>> CollectRemovableMethods(CSharpRemovableDeclarationCollector removableDeclarationCollector) =>
            removableDeclarationCollector.TypeDeclarations
                .SelectMany(x => x.Node.DescendantNodes(CSharpRemovableDeclarationCollector.IsNodeContainerTypeDeclaration)
                                    .OfType<MethodDeclarationSyntax>()
                                    .Select(y => new NodeSymbolAndModel<MethodDeclarationSyntax, IMethodSymbol>(y, x.Model.GetDeclaredSymbol(y), x.Model)))
                .Where(x => x.Symbol is { ReturnsVoid: false, IsAsync: false } && CSharpRemovableDeclarationCollector.IsRemovable(x.Symbol, Accessibility.Private));

    // Maps a ReducedExtension invocation back to the speakable form returned by GetDeclaredSymbol.
    private static IMethodSymbol Unreduce(IMethodSymbol method) =>
        method?.MethodKind == MethodKind.ReducedExtension
            ? method.ReducedFrom
            : method;

    // Extension-block members have two equivalent symbols (speakable and lowered implementation); invocations may resolve to either.
    private static bool MatchesDeclaration(IMethodSymbol invocation, IMethodSymbol declaration) =>
        invocation.OriginalDefinition.Equals(declaration)
        || (declaration.AssociatedExtensionImplementation is { } implementation && invocation.OriginalDefinition.Equals(implementation));
}
