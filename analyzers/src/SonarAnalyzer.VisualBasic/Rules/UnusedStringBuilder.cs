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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class UnusedStringBuilder : UnusedStringBuilderBase<SyntaxKind, VariableDeclaratorSyntax, IdentifierNameSyntax>
{
    private static readonly HashSet<SyntaxKind> SkipChildren = [];

    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override SyntaxNode Scope(VariableDeclaratorSyntax declarator) =>
        declarator is { Parent: LocalDeclarationStatementSyntax { Parent: { } block } }
            ? block
            : null;

    protected override ILocalSymbol RetrieveStringBuilderObject(SemanticModel model, VariableDeclaratorSyntax declarator) =>
        declarator is
        {
            Parent: LocalDeclarationStatementSyntax,
            Initializer.Value: ObjectCreationExpressionSyntax { } objectCreation,
            Names: { Count: 1 } names, // Must be 1, because otherwise "BC30671: Explicit initialization is not permitted with multiple variables declared with a single type specifier." is raised.
        }
        && objectCreation.Type.IsKnownType(KnownType.System_Text_StringBuilder, model)
            ? model.GetDeclaredSymbol(names[0]) as ILocalSymbol
            : null;

    protected override SyntaxNode StringBuilderReadExpression(SemanticModel model, SyntaxNode node) =>
        node switch
        {
            InvocationExpressionSyntax invocation when
                IsAccessInvocation(invocation.GetName()) || model.GetSymbolInfo(invocation).Symbol is IPropertySymbol { IsIndexer: true } => invocation.Expression,
            ReturnStatementSyntax returnStatement => returnStatement.Expression,
            InterpolationSyntax interpolation => interpolation.Expression,
            ArgumentSyntax argument => argument.GetExpression(),
            MemberAccessExpressionSyntax memberAccess when IsAccessExpression(memberAccess.Name.GetName()) => memberAccess.Expression,
            VariableDeclaratorSyntax { Initializer.Value: IdentifierNameSyntax identifier } => identifier,
            AssignmentStatementSyntax { Right: IdentifierNameSyntax identifier } => identifier,
            _ => null,
        };

    protected override bool DescendIntoChildren(SyntaxNode node) =>
        !node.IsAnyKind(SkipChildren);
}
