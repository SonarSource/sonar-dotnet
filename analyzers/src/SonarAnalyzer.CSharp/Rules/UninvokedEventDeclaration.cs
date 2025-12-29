/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UninvokedEventDeclaration : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3264";
    private const string MessageFormat = "Remove the unused event '{0}' or invoke it.";

    private static readonly Accessibility MaxAccessibility = Accessibility.Public;
    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly HashSet<SyntaxKind> EventSyntax = [SyntaxKind.EventFieldDeclaration];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSymbolAction(RaiseOnUninvokedEventDeclaration, SymbolKind.NamedType);

    private static void RaiseOnUninvokedEventDeclaration(SonarSymbolReportingContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (namedType.IsClassOrStruct() && namedType.ContainingType is null)
        {
            var removableDeclarationCollector = new CSharpRemovableDeclarationCollector(namedType, context.Compilation);
            var removableEventFields = removableDeclarationCollector.RemovableFieldLikeDeclarations(EventSyntax, MaxAccessibility).ToArray();
            if (removableEventFields.Any())
            {
                var usedSymbols = InvokedEventSymbols(removableDeclarationCollector).Concat(PossiblyCopiedSymbols(removableDeclarationCollector)).ToHashSet();
                foreach (var field in removableEventFields.Where(x => !usedSymbols.Contains(x.Symbol)))
                {
                    context.ReportIssue(Rule, Location(field.Node), field.Symbol.Name);
                }
            }
        }

        static Location Location(SyntaxNode node) =>
            node is VariableDeclaratorSyntax variableDeclarator
                ? variableDeclarator.Identifier.GetLocation()
                : ((EventDeclarationSyntax)node).Identifier.GetLocation();
    }

    private static IEnumerable<ISymbol> InvokedEventSymbols(CSharpRemovableDeclarationCollector removableDeclarationCollector)
    {
        var delegateInvocations = removableDeclarationCollector.TypeDeclarations
            .SelectMany(x => x.Node.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                .Select(invocation => new NodeSymbolAndModel<InvocationExpressionSyntax, IMethodSymbol>(invocation, (IMethodSymbol)x.Model.GetSymbolInfo(invocation).Symbol, x.Model)))
            .Where(x => x.Symbol is not null && IsDelegateInvocation(x.Symbol));

        var invokedEventSymbols = delegateInvocations
            .Select(x => new NodeAndModel<ExpressionSyntax>(EventExpressionFromInvocation(x.Node, x.Symbol), x.Model))
            .Select(x => new NodeSymbolAndModel<ExpressionSyntax, IEventSymbol>(x.Node, x.Model.GetSymbolInfo(x.Node).Symbol as IEventSymbol, x.Model))
            .Where(x => x.Symbol is not null)
            .Select(x => x.Symbol.OriginalDefinition);

        return invokedEventSymbols;
    }

    private static IEnumerable<ISymbol> PossiblyCopiedSymbols(CSharpRemovableDeclarationCollector removableDeclarationCollector)
    {
        var usedSymbols = new List<ISymbol>();
        foreach (var typeDeclaration in removableDeclarationCollector.TypeDeclarations)
        {
            foreach (var node in typeDeclaration.Node.DescendantNodes().Select(Expression).WhereNotNull())
            {
                if (typeDeclaration.Model.GetSymbolInfo(node).Symbol is IEventSymbol symbol)
                {
                    usedSymbols.Add(symbol.OriginalDefinition);
                }
            }
        }
        return usedSymbols;

        static SyntaxNode Expression(SyntaxNode node) =>
            node switch
            {
                ArgumentSyntax arg => arg.Expression,
                EqualsValueClauseSyntax equalsClause => equalsClause.Value,
                AssignmentExpressionSyntax assignment => assignment.Right,
                _ => null
            };
    }

    private static ExpressionSyntax EventExpressionFromInvocation(InvocationExpressionSyntax invocation, IMethodSymbol symbol)
    {
        var expression = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => new NodeAndName(memberAccess.Expression, memberAccess.Name),
            MemberBindingExpressionSyntax memberBinding => new NodeAndName((invocation.Parent as ConditionalAccessExpressionSyntax)?.Expression, memberBinding.Name),
            _ => default
        };
        return expression.Node is not null && IsExplicitDelegateInvocation(symbol, expression.Name)
            ? expression.Node
            : invocation.Expression;
    }

    private static bool IsExplicitDelegateInvocation(IMethodSymbol symbol, SimpleNameSyntax invokedMethodName) =>
        IsDynamicInvoke(symbol)
        || IsBeginInvoke(symbol)
        || (symbol.MethodKind == MethodKind.DelegateInvoke && invokedMethodName.Identifier.ValueText == "Invoke");

    private static bool IsDelegateInvocation(IMethodSymbol symbol) =>
        symbol.MethodKind == MethodKind.DelegateInvoke
        || IsInvoke(symbol)
        || IsDynamicInvoke(symbol)
        || IsBeginInvoke(symbol);

    private static bool IsInvoke(IMethodSymbol symbol) =>
        symbol.MethodKind == MethodKind.Ordinary
        && symbol.Name == nameof(EventHandler.Invoke);

    private static bool IsDynamicInvoke(IMethodSymbol symbol) =>
        symbol.MethodKind == MethodKind.Ordinary
        && symbol.Name == nameof(Delegate.DynamicInvoke)
        && symbol.ReceiverType.OriginalDefinition.Is(KnownType.System_Delegate);

    private static bool IsBeginInvoke(IMethodSymbol symbol) =>
        symbol.MethodKind == MethodKind.Ordinary
        && symbol.Name == nameof(EventHandler.BeginInvoke);
}

file record struct NodeAndName(ExpressionSyntax Node, SimpleNameSyntax Name);
