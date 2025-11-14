/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public sealed class ReturnEmptyCollectionInsteadOfNull : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1168";
    private const string MessageFormat = "Return an empty collection instead of null.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableArray<KnownType> CollectionTypes = ImmutableArray.Create(
        KnownType.System_Collections_IEnumerable,
        KnownType.System_Array);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            ReportIfReturnsNullOrDefault,
            SyntaxKind.MethodDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.ConversionOperatorDeclaration);

    private static void ReportIfReturnsNullOrDefault(SonarSyntaxNodeReportingContext context)
    {
        if (ExpressionBody(context.Node) is { } expressionBody)
        {
            var nullOrDefaultLiterals = NullOrDefaultExpressions(expressionBody.Expression)
                .Select(x => x.GetLocation())
                .ToList();

            ReportIfAny(nullOrDefaultLiterals);
        }
        else if (Body(context.Node) is { } body)
        {
            var nullOrDefaultLiterals = ReturnNullOrDefaultExpressions(body)
                .Select(x => x.GetLocation())
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

    private static BlockSyntax Body(SyntaxNode node) =>
        node is BasePropertyDeclarationSyntax property ? GetAccessor(property)?.Body : node.GetBody();

    private static bool IsReturningCollection(SonarSyntaxNodeReportingContext context) =>
        DeclaredType(context) is { } type
        && !type.Is(KnownType.System_String)
        && !type.DerivesFrom(KnownType.System_Xml_XmlNode)
        && type.DerivesOrImplementsAny(CollectionTypes)
        && type.NullableAnnotation() != NullableAnnotation.Annotated;

    private static ITypeSymbol DeclaredType(SonarSyntaxNodeReportingContext context)
    {
        var symbol = context.Model.GetDeclaredSymbol(context.Node);
        return symbol is IPropertySymbol property ? property.Type : ((IMethodSymbol)symbol).ReturnType;
    }

    private static ArrowExpressionClauseSyntax ExpressionBody(SyntaxNode node) =>
        node switch
        {
            BaseMethodDeclarationSyntax method => method.ExpressionBody(),
            BasePropertyDeclarationSyntax property => property.ArrowExpressionBody() ?? GetAccessor(property)?.ExpressionBody(),
            _ => ((LocalFunctionStatementSyntaxWrapper)node).ExpressionBody,
        };

    private static AccessorDeclarationSyntax GetAccessor(BasePropertyDeclarationSyntax property) =>
        property.AccessorList.Accessors.FirstOrDefault(x => x.IsKind(SyntaxKind.GetAccessorDeclaration));

    private static IEnumerable<SyntaxNode> ReturnNullOrDefaultExpressions(SyntaxNode methodBlock) =>
        methodBlock.DescendantNodes(x =>
            !(x.Kind() is SyntaxKindEx.LocalFunctionStatement
                or SyntaxKind.SimpleLambdaExpression
                or SyntaxKind.ParenthesizedLambdaExpression))
            .OfType<ReturnStatementSyntax>()
            .SelectMany(x => NullOrDefaultExpressions(x.Expression));

    private static IEnumerable<SyntaxNode> NullOrDefaultExpressions(SyntaxNode node)
    {
        node = node.RemoveParentheses();
        if (node.IsNullLiteral() || node?.Kind() is SyntaxKindEx.DefaultLiteralExpression or SyntaxKind.DefaultExpression)
        {
            yield return node;
            yield break;
        }
        if (node is ConditionalExpressionSyntax c)
        {
            foreach (var innerNode in NullOrDefaultExpressions(c.WhenTrue))
            {
                yield return innerNode;
            }
            foreach (var innerNode in NullOrDefaultExpressions(c.WhenFalse))
            {
                yield return innerNode;
            }
        }
    }
}
