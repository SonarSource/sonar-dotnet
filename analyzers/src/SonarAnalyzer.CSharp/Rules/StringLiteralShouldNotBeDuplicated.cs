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
public sealed class StringLiteralShouldNotBeDuplicated : StringLiteralShouldNotBeDuplicatedBase<SyntaxKind, LiteralExpressionSyntax>
{
    private static readonly HashSet<SyntaxKind> TypeDeclarationSyntaxKinds =
    [
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration,
        SyntaxKindEx.RecordDeclaration,
        SyntaxKindEx.RecordStructDeclaration
    ];

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override SyntaxKind[] SyntaxKinds { get; } =
    [
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration,
        SyntaxKindEx.RecordDeclaration,
        SyntaxKindEx.RecordStructDeclaration,
        SyntaxKind.CompilationUnit
    ];

    protected override bool IsMatchingMethodParameterName(LiteralExpressionSyntax literalExpression) =>
        literalExpression.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>()
            ?.ParameterList
            ?.Parameters
            .Any(x => x.Identifier.ValueText == literalExpression.Token.ValueText)
        ?? false;

    protected override bool IsInModelConfigurationContext(LiteralExpressionSyntax literalExpression, SemanticModel model) =>
        literalExpression.FirstAncestorOrSelf<MethodDeclarationSyntax>() is { } methodDeclaration
        && model.GetDeclaredSymbol(methodDeclaration) is { } methodSymbol
        && IsModelConfigurationMethod(methodSymbol);

    protected override bool IsInnerInstance(SonarSyntaxNodeReportingContext context) =>
        context.Node.Ancestors().Any(x =>
            x.IsAnyKind(TypeDeclarationSyntaxKinds)
            || (x.IsKind(SyntaxKind.CompilationUnit) && x.ChildNodes().Any(y => y.IsKind(SyntaxKind.GlobalStatement))));

    protected override IEnumerable<LiteralExpressionSyntax> FindLiteralExpressions(SyntaxNode node) =>
        node.DescendantNodes(x => !x.IsKind(SyntaxKind.AttributeList))
            .Where(x => x.IsKind(SyntaxKind.StringLiteralExpression))
            .Cast<LiteralExpressionSyntax>();

    protected override SyntaxToken LiteralToken(LiteralExpressionSyntax literal) =>
        literal.Token;

    protected override bool IsNamedTypeOrTopLevelMain(SonarSyntaxNodeReportingContext context) =>
        IsNamedType(context) || context.IsTopLevelMain;
}
