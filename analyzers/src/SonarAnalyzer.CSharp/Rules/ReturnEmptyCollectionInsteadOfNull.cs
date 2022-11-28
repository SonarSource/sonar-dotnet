/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
    public sealed class ReturnEmptyCollectionInsteadOfNull : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1168";
        private const string MessageFormat = "Return an empty collection instead of null.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<KnownType> CollectionTypes = ImmutableArray.Create(
            KnownType.System_Collections_IEnumerable,
            KnownType.System_Array);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(ReportIfReturnsNullOrDefault,
                SyntaxKind.MethodDeclaration,
                SyntaxKindEx.LocalFunctionStatement,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.OperatorDeclaration);

        private static void ReportIfReturnsNullOrDefault(SyntaxNodeAnalysisContext context)
        {
            var expressionBody = GetExpressionBody(context.Node);
            if (expressionBody != null)
            {
                var nullOrDefaultLiterals = GetNullOrDefaultExpressions(expressionBody.Expression)
                    .Select(statement => statement.GetLocation())
                    .ToList();

                if (nullOrDefaultLiterals.Count > 0 && IsReturningCollection(context))
                {
                    context.ReportIssue(Rule.CreateDiagnostic(context.Compilation, nullOrDefaultLiterals[0], additionalLocations: nullOrDefaultLiterals.Skip(1)));
                }

                return;
            }

            var body = GetBody(context.Node);
            if (body != null)
            {
                var nullOrDefaultLiterals = GetReturnNullOrDefaultExpressions(body)
                    .Select(returnStatement => returnStatement.GetLocation())
                    .ToList();

                if (nullOrDefaultLiterals.Count > 0 && IsReturningCollection(context))
                {
                    context.ReportIssue(Rule.CreateDiagnostic(context.Compilation, nullOrDefaultLiterals[0], additionalLocations: nullOrDefaultLiterals.Skip(1)));
                }
            }
        }

        private static bool IsReturningCollection(SyntaxNodeAnalysisContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);

            var methodSymbol = (symbol as IPropertySymbol)?.GetMethod ?? symbol as IMethodSymbol;

            return methodSymbol != null &&
                !methodSymbol.ReturnType.Is(KnownType.System_String) &&
                !methodSymbol.ReturnType.DerivesFrom(KnownType.System_Xml_XmlNode) &&
                methodSymbol.ReturnType.DerivesOrImplementsAny(CollectionTypes);
        }

        private static ArrowExpressionClauseSyntax GetExpressionBody(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)node).ExpressionBody;

                case SyntaxKind.PropertyDeclaration:
                    var property = (PropertyDeclarationSyntax)node;

                    if (property.ExpressionBody != null)
                    {
                        return property.ExpressionBody;
                    }

                    return property.AccessorList?.Accessors
                        .FirstOrDefault(accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                        ?.ExpressionBody();

                case SyntaxKindEx.LocalFunctionStatement:
                    return ((LocalFunctionStatementSyntaxWrapper)node).ExpressionBody;

                case SyntaxKind.OperatorDeclaration:
                    return ((OperatorDeclarationSyntax)node).ExpressionBody;
                default:
                    return null;
            }
        }

        private static BlockSyntax GetBody(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)node).Body;

                case SyntaxKind.PropertyDeclaration:
                    return ((PropertyDeclarationSyntax)node).AccessorList?.Accessors
                        .FirstOrDefault(accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                        ?.Body;

                case SyntaxKindEx.LocalFunctionStatement:
                    return ((LocalFunctionStatementSyntaxWrapper)node).Body;

                case SyntaxKind.OperatorDeclaration:
                    return ((OperatorDeclarationSyntax)node).Body;

                default:
                    return null;
            }
        }

        private static IEnumerable<SyntaxNode> GetReturnNullOrDefaultExpressions(SyntaxNode methodBlock) =>
            methodBlock.DescendantNodes(n =>
                    !n.IsAnyKind(SyntaxKindEx.LocalFunctionStatement,
                        SyntaxKind.SimpleLambdaExpression,
                        SyntaxKind.ParenthesizedLambdaExpression))
                   .OfType<ReturnStatementSyntax>()
                   .SelectMany(statement => GetNullOrDefaultExpressions(statement.Expression));

        private static IEnumerable<SyntaxNode> GetNullOrDefaultExpressions(SyntaxNode node)
        {
            node = node.RemoveParentheses();

            if (node.IsNullLiteral() || node.IsKind(SyntaxKindEx.DefaultLiteralExpression))
            {
                yield return node;
                yield break;
            }

            if (node is ConditionalExpressionSyntax c)
            {
                foreach (var innerNode in GetNullOrDefaultExpressions(c.WhenTrue))
                {
                    yield return innerNode;
                }

                foreach (var innerNode in GetNullOrDefaultExpressions(c.WhenFalse))
                {
                    yield return innerNode;
                }
            }
        }
    }
}
