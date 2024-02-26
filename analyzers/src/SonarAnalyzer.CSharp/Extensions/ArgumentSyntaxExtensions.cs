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

namespace SonarAnalyzer.Extensions
{
    internal static class ArgumentSyntaxExtensions
    {
        internal static int? GetArgumentIndex(this ArgumentSyntax argument) =>
            (argument.Parent as ArgumentListSyntax)?.Arguments.IndexOf(argument);

        internal static IEnumerable<ArgumentSyntax> GetArgumentsOfKnownType(this SeparatedSyntaxList<ArgumentSyntax> syntaxList, KnownType knownType, SemanticModel semanticModel) =>
            syntaxList
                .Where(argument => semanticModel.GetTypeInfo(argument.Expression).Type.Is(knownType));

        internal static IEnumerable<ISymbol> GetSymbolsOfKnownType(this SeparatedSyntaxList<ArgumentSyntax> syntaxList, KnownType knownType, SemanticModel semanticModel) =>
            syntaxList
                .GetArgumentsOfKnownType(knownType, semanticModel)
                .Select(argument => semanticModel.GetSymbolInfo(argument.Expression).Symbol);

        internal static bool NameIs(this ArgumentSyntax argument, string name) =>
            argument.NameColon?.Name.Identifier.Text == name;

        // (arg, b) = something
        internal static bool IsInTupleAssignmentTarget(this ArgumentSyntax argument) =>
            argument.OutermostTuple() is { SyntaxNode: { } tupleExpression }
            && tupleExpression.Parent is AssignmentExpressionSyntax assignment
            && assignment.Left == tupleExpression;

        internal static TupleExpressionSyntaxWrapper? OutermostTuple(this ArgumentSyntax argument) =>
            argument.Ancestors()
                .TakeWhile(x => x.IsAnyKind(SyntaxKind.Argument, SyntaxKindEx.TupleExpression))
                .LastOrDefault(x => x.IsKind(SyntaxKindEx.TupleExpression)) is { } outerTuple
                && TupleExpressionSyntaxWrapper.IsInstance(outerTuple)
                    ? (TupleExpressionSyntaxWrapper)outerTuple
                    : null;
    }
}
