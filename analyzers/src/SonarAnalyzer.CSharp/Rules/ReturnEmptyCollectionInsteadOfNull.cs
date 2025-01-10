/*
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
                context.ReportIssue(Rule, nullOrDefaultLiterals[0], nullOrDefaultLiterals.Skip(1).ToSecondary(MessageFormat));
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
        methodBlock.DescendantNodes(x =>
                !(x.Kind() is
                    SyntaxKindEx.LocalFunctionStatement or
                    SyntaxKind.SimpleLambdaExpression or
                    SyntaxKind.ParenthesizedLambdaExpression))
               .OfType<ReturnStatementSyntax>()
               .SelectMany(x => GetNullOrDefaultExpressions(x.Expression));

    private static IEnumerable<SyntaxNode> GetNullOrDefaultExpressions(SyntaxNode node)
    {
        node = node.RemoveParentheses();

        if (node.IsNullLiteral() || node?.Kind() is SyntaxKindEx.DefaultLiteralExpression or SyntaxKind.DefaultExpression)
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
