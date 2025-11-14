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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class ArgumentSyntaxExtensions
{
    public static int? GetArgumentIndex(this ArgumentSyntax argument) =>
        (argument.Parent as ArgumentListSyntax)?.Arguments.IndexOf(argument);

    public static IEnumerable<ArgumentSyntax> GetArgumentsOfKnownType(this SeparatedSyntaxList<ArgumentSyntax> syntaxList, KnownType knownType, SemanticModel semanticModel) =>
        syntaxList
            .Where(argument => semanticModel.GetTypeInfo(argument.Expression).Type.Is(knownType));

    public static IEnumerable<ISymbol> GetSymbolsOfKnownType(this SeparatedSyntaxList<ArgumentSyntax> syntaxList, KnownType knownType, SemanticModel semanticModel) =>
        syntaxList
            .GetArgumentsOfKnownType(knownType, semanticModel)
            .Select(argument => semanticModel.GetSymbolInfo(argument.Expression).Symbol);

    public static bool NameIs(this ArgumentSyntax argument, string name) =>
        argument.NameColon?.Name.Identifier.Text == name;

    // (arg, b) = something
    public static bool IsInTupleAssignmentTarget(this ArgumentSyntax argument) =>
        argument.OutermostTuple() is { SyntaxNode: { } tupleExpression }
        && tupleExpression.Parent is AssignmentExpressionSyntax assignment
        && assignment.Left == tupleExpression;

    public static TupleExpressionSyntaxWrapper? OutermostTuple(this ArgumentSyntax argument) =>
        argument.Ancestors()
            .TakeWhile(x => x?.Kind() is SyntaxKind.Argument or SyntaxKindEx.TupleExpression)
            .LastOrDefault(x => x.IsKind(SyntaxKindEx.TupleExpression)) is { } outerTuple
            && TupleExpressionSyntaxWrapper.IsInstance(outerTuple)
                ? (TupleExpressionSyntaxWrapper)outerTuple
                : null;
}
