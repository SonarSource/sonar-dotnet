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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Core.Trackers;

public class CSharpConstantValueFinder : ConstantValueFinder<IdentifierNameSyntax, VariableDeclaratorSyntax>
{
    public CSharpConstantValueFinder(SemanticModel semanticModel) : base(semanticModel, new CSharpAssignmentFinder(), (int)SyntaxKind.NullLiteralExpression) { }

    protected override string IdentifierName(IdentifierNameSyntax node) =>
        node.Identifier.ValueText;

    protected override SyntaxNode InitializerValue(VariableDeclaratorSyntax node) =>
        node.Initializer?.Value;

    protected override VariableDeclaratorSyntax VariableDeclarator(SyntaxNode node) =>
        node as VariableDeclaratorSyntax;

    protected override bool IsPtrZero(SyntaxNode node) =>
        node is MemberAccessExpressionSyntax memberAccess
        && memberAccess.IsPtrZero(Model);
}
