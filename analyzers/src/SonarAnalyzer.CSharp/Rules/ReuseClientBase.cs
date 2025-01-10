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

public abstract class ReuseClientBase : SonarDiagnosticAnalyzer
{
    private static readonly HashSet<SyntaxKind> ConditionalKinds =
        [
            SyntaxKind.IfStatement,
            SyntaxKind.SwitchStatement,
            SyntaxKindEx.SwitchExpression,
            SyntaxKind.ConditionalExpression,
            SyntaxKindEx.CoalesceAssignmentExpression
        ];

    protected abstract ImmutableArray<KnownType> ReusableClients { get; }

    protected static bool IsAssignedForReuse(SonarSyntaxNodeReportingContext context) =>
        !IsInVariableDeclaration(context.Node)
        && (IsInConditionalCode(context.Node) || IsInFieldOrPropertyInitializer(context.Node) || IsAssignedToStaticFieldOrProperty(context));

    protected bool IsReusableClient(SonarSyntaxNodeReportingContext context)
    {
        var objectCreation = ObjectCreationFactory.Create(context.Node);
        return ReusableClients.Any(x => objectCreation.IsKnownType(x, context.SemanticModel));
    }

    private static bool IsInVariableDeclaration(SyntaxNode node) =>
        node.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax or UsingStatementSyntax } } };

    private static bool IsInFieldOrPropertyInitializer(SyntaxNode node) =>
        node.HasAncestor(SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration)
        && !node.HasAncestor(SyntaxKind.GetAccessorDeclaration, SyntaxKind.SetAccessorDeclaration)
        && !node.Parent.IsKind(SyntaxKind.ArrowExpressionClause);

    private static bool IsInConditionalCode(SyntaxNode node) =>
        node.HasAncestor(ConditionalKinds);

    private static bool IsAssignedToStaticFieldOrProperty(SonarSyntaxNodeReportingContext context) =>
        context.Node.Parent.WalkUpParentheses() is AssignmentExpressionSyntax assignment
        && context.SemanticModel.GetSymbolInfo(assignment.Left, context.Cancel).Symbol is { IsStatic: true, Kind: SymbolKind.Field or SymbolKind.Property };
}
