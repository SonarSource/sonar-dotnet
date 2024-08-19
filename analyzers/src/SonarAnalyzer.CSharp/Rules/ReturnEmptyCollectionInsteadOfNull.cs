/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp;

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
        context.RegisterNodeAction(ReportIfReturnsNullOrDefault,
            SyntaxKind.MethodDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.OperatorDeclaration);

    private static void ReportIfReturnsNullOrDefault(SonarSyntaxNodeReportingContext context)
    {
        if (GetExpressionBody(context.Node) is { } expressionBody)
        {
            var nullOrDefaultLiterals = GetNullOrDefaultExpressions(expressionBody.Expression)
                .Select(statement => statement.GetLocation())
                .ToList();

            ReportIfAny(nullOrDefaultLiterals);
        }
        else if (GetBody(context.Node) is { } body)
        {
            var nullOrDefaultLiterals = GetReturnNullOrDefaultExpressions(body)
                .Select(returnStatement => returnStatement.GetLocation())
                .ToList();

            ReportIfAny(nullOrDefaultLiterals);
        }

        void ReportIfAny(List<Location> nullOrDefaultLiterals)
        {
            if (nullOrDefaultLiterals.Count > 0 && IsReturningCollection(context))
            {
                context.ReportIssue(Rule, nullOrDefaultLiterals[0], nullOrDefaultLiterals.Skip(1).ToSecondary());
            }
        }
    }

    private static BlockSyntax GetBody(SyntaxNode node) =>
        node is PropertyDeclarationSyntax property ? GetAccessor(property)?.Body : node.GetBody();

    private static bool IsReturningCollection(SonarSyntaxNodeReportingContext context) =>
        GetType(context) is { } type
        && !type.Is(KnownType.System_String)
        && !type.DerivesFrom(KnownType.System_Xml_XmlNode)
        && type.DerivesOrImplementsAny(CollectionTypes)
        && type.NullableAnnotation() != NullableAnnotation.Annotated;

    private static ITypeSymbol GetType(SonarSyntaxNodeReportingContext context)
    {
        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
        return symbol is IPropertySymbol property ? property.Type : ((IMethodSymbol)symbol).ReturnType;
    }

    private static ArrowExpressionClauseSyntax GetExpressionBody(SyntaxNode node) =>
        node switch
        {
            BaseMethodDeclarationSyntax method => method.ExpressionBody(),
            PropertyDeclarationSyntax property => property.ExpressionBody ?? GetAccessor(property)?.ExpressionBody(),
            _ => ((LocalFunctionStatementSyntaxWrapper)node).ExpressionBody,
        };

    private static AccessorDeclarationSyntax GetAccessor(PropertyDeclarationSyntax property) =>
        property.AccessorList.Accessors.FirstOrDefault(x => x.IsKind(SyntaxKind.GetAccessorDeclaration));

    private static IEnumerable<SyntaxNode> GetReturnNullOrDefaultExpressions(SyntaxNode methodBlock) =>
        methodBlock.DescendantNodes(n =>
                !n.IsAnyKind(
                    SyntaxKindEx.LocalFunctionStatement,
                    SyntaxKind.SimpleLambdaExpression,
                    SyntaxKind.ParenthesizedLambdaExpression))
               .OfType<ReturnStatementSyntax>()
               .SelectMany(statement => GetNullOrDefaultExpressions(statement.Expression));

    private static IEnumerable<SyntaxNode> GetNullOrDefaultExpressions(SyntaxNode node)
    {
        node = node.RemoveParentheses();

        if (node.IsNullLiteral() || node.IsAnyKind(SyntaxKindEx.DefaultLiteralExpression, SyntaxKind.DefaultExpression))
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
