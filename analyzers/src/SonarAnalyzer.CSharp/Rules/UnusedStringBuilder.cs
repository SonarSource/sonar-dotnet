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
public sealed class UnusedStringBuilder : UnusedStringBuilderBase<SyntaxKind, VariableDeclaratorSyntax, IdentifierNameSyntax>
{
    private static readonly HashSet<SyntaxKind> SkipChildren =
    [
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration,
        SyntaxKind.EnumDeclaration,
        SyntaxKind.InterfaceDeclaration,
        SyntaxKind.UsingDirective,
        SyntaxKindEx.RecordDeclaration,
        SyntaxKindEx.RecordStructDeclaration
    ];

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override SyntaxNode GetScope(VariableDeclaratorSyntax declarator) =>
        declarator switch
        {
            { Parent: VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax { Parent: BlockSyntax block } } } => block,
            { Parent: VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax { Parent: GlobalStatementSyntax { Parent: CompilationUnitSyntax compilationUnit } } } } => compilationUnit,
            _ => null,
        };

    protected override ILocalSymbol RetrieveStringBuilderObject(SemanticModel semanticModel, VariableDeclaratorSyntax declarator) =>
        declarator is
        {
            Parent.Parent: LocalDeclarationStatementSyntax,
            Initializer.Value: { } expression,
        }
        && IsStringBuilderObjectCreation(expression, semanticModel)
            ? semanticModel.GetDeclaredSymbol(declarator) as ILocalSymbol
            : null;

    protected override bool IsStringBuilderRead(SemanticModel model, ILocalSymbol symbol, SyntaxNode node) =>
        node switch
        {
            InvocationExpressionSyntax invocation => IsAccessInvocation(invocation.Expression.GetName()) && IsSameReference(model, symbol, invocation.Expression),
            ReturnStatementSyntax returnStatement => IsSameReference(model, symbol, returnStatement.Expression),
            InterpolationSyntax interpolation => IsSameReference(model, symbol, interpolation.Expression),
            ElementAccessExpressionSyntax elementAccess => IsSameReference(model, symbol, elementAccess.Expression),
            ArgumentSyntax argument => IsSameReference(model, symbol, argument.Expression),
            MemberAccessExpressionSyntax memberAccess => IsAccessExpression(memberAccess.Name.GetName()) && IsSameReference(model, symbol, memberAccess.Expression),
            VariableDeclaratorSyntax { Initializer.Value: IdentifierNameSyntax identifier } => IsSameReference(model, symbol, identifier),
            AssignmentExpressionSyntax { Right: IdentifierNameSyntax identifier } => IsSameReference(model, symbol, identifier),
            BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression, Left: IdentifierNameSyntax identifier } => IsSameReference(model, symbol, identifier),
            BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression, Right: IdentifierNameSyntax identifier } => IsSameReference(model, symbol, identifier),
            _ => false,
        };

    protected override bool DescendIntoChildren(SyntaxNode node) =>
        !(node.IsAnyKind(SkipChildren)
        || (node.IsKind(SyntaxKindEx.LocalFunctionStatement) && ((LocalFunctionStatementSyntaxWrapper)node).Modifiers.Any(SyntaxKind.StaticKeyword)));

    private static bool IsStringBuilderObjectCreation(ExpressionSyntax expression, SemanticModel model) =>
        ObjectCreationFactory.TryCreate(expression) is { } creation
        && creation.IsKnownType(KnownType.System_Text_StringBuilder, model);
}
