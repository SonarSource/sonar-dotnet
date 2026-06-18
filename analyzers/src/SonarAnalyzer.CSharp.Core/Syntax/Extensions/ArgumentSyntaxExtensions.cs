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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class ArgumentSyntaxExtensions
{
    extension(ArgumentSyntax argument)
    {
        public int? ArgumentIndex =>
            (argument.Parent as ArgumentListSyntax)?.Arguments.IndexOf(argument);

        public bool NameIs(string name) =>
            argument.NameColon?.Name.Identifier.Text == name;

        // (arg, b) = something
        public bool IsInTupleAssignmentTarget =>
            argument.OutermostTuple is { SyntaxNode: { } tupleExpression }
            && tupleExpression.Parent is AssignmentExpressionSyntax assignment
            && assignment.Left == tupleExpression;

        public TupleExpressionSyntaxWrapper? OutermostTuple =>
            argument.Ancestors()
                .TakeWhile(x => x?.Kind() is SyntaxKind.Argument or SyntaxKindEx.TupleExpression)
                .LastOrDefault(x => x.IsKind(SyntaxKindEx.TupleExpression)) is { } outerTuple
                && TupleExpressionSyntaxWrapper.IsInstance(outerTuple)
                    ? (TupleExpressionSyntaxWrapper)outerTuple
                    : null;
    }

    extension(SeparatedSyntaxList<ArgumentSyntax> syntaxList)
    {
        public IEnumerable<ArgumentSyntax> GetArgumentsOfKnownType(KnownType knownType, SemanticModel semanticModel) =>
            syntaxList
                .Where(argument => semanticModel.GetTypeInfo(argument.Expression).Type.Is(knownType));

        public IEnumerable<ISymbol> GetSymbolsOfKnownType(KnownType knownType, SemanticModel semanticModel) =>
            syntaxList
                .GetArgumentsOfKnownType(knownType, semanticModel)
                .Select(argument => semanticModel.GetSymbolInfo(argument.Expression).Symbol);
    }
}
