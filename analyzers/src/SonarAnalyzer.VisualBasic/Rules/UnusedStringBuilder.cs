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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class UnusedStringBuilder : UnusedStringBuilderBase<SyntaxKind, VariableDeclaratorSyntax, IdentifierNameSyntax>
{
    private static readonly SyntaxKind[] SkipChildren = { };

    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override SyntaxNode GetScope(VariableDeclaratorSyntax declarator) =>
        declarator is { Parent: LocalDeclarationStatementSyntax { Parent: { } block } }
        ? block
        : null;

    protected override ILocalSymbol RetrieveStringBuilderObject(SemanticModel semanticModel, VariableDeclaratorSyntax declarator) =>
        declarator is
        {
            Parent: LocalDeclarationStatementSyntax,
            Initializer.Value: ObjectCreationExpressionSyntax { } objectCreation,
            Names: { Count: 1 } names, // Must be 1, because otherwise "BC30671: Explicit initialization is not permitted with multiple variables declared with a single type specifier." is raised.
        }
        && objectCreation.Type.IsKnownType(KnownType.System_Text_StringBuilder, semanticModel)
            ? semanticModel.GetDeclaredSymbol(names[0]) as ILocalSymbol
            : null;

    protected override bool IsStringBuilderRead(SemanticModel model, ILocalSymbol symbol, SyntaxNode node) =>
        node switch
        {
            InvocationExpressionSyntax invocation =>
                (IsAccessInvocation(invocation.GetName()) && IsSameReference(model, symbol, invocation.Expression))
                || (IsSameReference(model, symbol, invocation.Expression) && model.GetSymbolInfo(invocation).Symbol is IPropertySymbol { IsIndexer: true }),
            ReturnStatementSyntax returnStatement => IsSameReference(model, symbol, returnStatement.Expression),
            InterpolationSyntax interpolation => IsSameReference(model, symbol, interpolation.Expression),
            ArgumentSyntax argument => IsSameReference(model, symbol, argument.GetExpression()),
            MemberAccessExpressionSyntax memberAccess => IsAccessExpression(memberAccess.Name.GetName()) && IsSameReference(model, symbol, memberAccess.Expression),
            VariableDeclaratorSyntax { Initializer.Value: IdentifierNameSyntax identifier } => IsSameReference(model, symbol, identifier),
            AssignmentStatementSyntax { Right: IdentifierNameSyntax identifier } => IsSameReference(model, symbol, identifier),
            _ => false,
        };

    protected override bool DescendIntoChildren(SyntaxNode node) =>
        !node.IsAnyKind(SkipChildren);
}
