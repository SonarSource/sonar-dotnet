/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UninvokedEventDeclaration : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3264";
        private const string MessageFormat = "Remove the unused event '{0}' or invoke it.";

        private static readonly Accessibility MaxAccessibility = Accessibility.Public;
        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly ISet<SyntaxKind> EventSyntax = new HashSet<SyntaxKind>
        {
            SyntaxKind.EventFieldDeclaration,
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(RaiseOnUninvokedEventDeclaration, SymbolKind.NamedType);

        private void RaiseOnUninvokedEventDeclaration(SonarSymbolReportingContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            if (!namedType.IsClassOrStruct() || namedType.ContainingType != null)
            {
                return;
            }

            var removableDeclarationCollector = new CSharpRemovableDeclarationCollector(namedType, context.Compilation);

            var removableEventFields = removableDeclarationCollector
                .GetRemovableFieldLikeDeclarations(EventSyntax, MaxAccessibility)
                .ToList();

            if (!removableEventFields.Any())
            {
                return;
            }

            var usedSymbols = GetInvokedEventSymbols(removableDeclarationCollector)
                .Concat(GetPossiblyCopiedSymbols(removableDeclarationCollector))
                .ToHashSet();

            removableEventFields
                .Where(x => !usedSymbols.Contains(x.Symbol))
                .ToList()
                .ForEach(x => context.ReportIssue(CreateDiagnostic(Rule, GetLocation(x.Node), x.Symbol.Name)));

            Location GetLocation(SyntaxNode node) =>
                node is VariableDeclaratorSyntax variableDeclarator
                    ? variableDeclarator.Identifier.GetLocation()
                    : ((EventDeclarationSyntax)node).Identifier.GetLocation();
        }

        private static IEnumerable<ISymbol> GetInvokedEventSymbols(CSharpRemovableDeclarationCollector removableDeclarationCollector)
        {
            var delegateInvocations = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.Node.DescendantNodes()
                    .Where(x => x.IsKind(SyntaxKind.InvocationExpression))
                    .Cast<InvocationExpressionSyntax>()
                    .Select(x => new NodeSymbolAndModel<InvocationExpressionSyntax, IMethodSymbol>(container.Model, x, container.Model.GetSymbolInfo(x).Symbol as IMethodSymbol)))
                 .Where(x => x.Symbol != null && IsDelegateInvocation(x.Symbol));

            var invokedEventSymbols = delegateInvocations
                .Select(x => new NodeAndModel<ExpressionSyntax>(x.Model, GetEventExpressionFromInvocation(x.Node, x.Symbol)))
                .Select(x => new NodeSymbolAndModel<ExpressionSyntax, IEventSymbol>(x.Model, x.Node, x.Model.GetSymbolInfo(x.Node).Symbol as IEventSymbol))
                .Where(x => x.Symbol != null)
                .Select(x => x.Symbol.OriginalDefinition);

            return invokedEventSymbols;
        }

        private static IEnumerable<ISymbol> GetPossiblyCopiedSymbols(CSharpRemovableDeclarationCollector removableDeclarationCollector)
        {
            var arguments = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.Node.DescendantNodes()
                    .Where(x => x.IsKind(SyntaxKind.Argument))
                    .Cast<ArgumentSyntax>()
                    .Select(x => new NodeAndModel<SyntaxNode>(container.Model, x.Expression)));

            var equalsValue = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.Node.DescendantNodes()
                    .OfType<EqualsValueClauseSyntax>()
                    .Select(x => new NodeAndModel<SyntaxNode>(container.Model, x.Value)));

            var assignment = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.Node.DescendantNodes()
                    .Where(x => x.IsKind(SyntaxKind.SimpleAssignmentExpression))
                    .Cast<AssignmentExpressionSyntax>()
                    .Select(x => new NodeAndModel<SyntaxNode>(container.Model, x.Right)));

            var allNodes = arguments.Concat(equalsValue).Concat(assignment);

            var usedSymbols = new List<ISymbol>();
            foreach (var node in allNodes)
            {
                if (node.Model.GetSymbolInfo(node.Node).Symbol is IEventSymbol symbol)
                {
                    usedSymbols.Add(symbol.OriginalDefinition);
                }
            }

            return usedSymbols;
        }

        private static ExpressionSyntax GetEventExpressionFromInvocation(InvocationExpressionSyntax invocation, IMethodSymbol symbol)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            var memberBinding = invocation.Expression as MemberBindingExpressionSyntax;

            var expression = memberAccess?.Expression;
            var invokedMethodName = memberAccess?.Name;
            if (memberBinding != null)
            {
                expression = (invocation.Parent as ConditionalAccessExpressionSyntax)?.Expression;
                invokedMethodName = memberBinding.Name;
            }

            return (memberAccess != null || memberBinding != null)
                   && expression != null
                   && IsExplicitDelegateInvocation(symbol, invokedMethodName)
                ? expression
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
}
