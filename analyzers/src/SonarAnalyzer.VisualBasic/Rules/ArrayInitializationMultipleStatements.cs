/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ArrayInitializationMultipleStatements : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2429";
        private const string MessageFormat = "Refactor this code to use the '... = {}' syntax.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var declaration = (LocalDeclarationStatementSyntax)c.Node;
                    if (declaration.Declarators.Count != 1)
                    {
                        return;
                    }

                    var declarator = declaration.Declarators.First();
                    if (declarator.Names.Count != 1)
                    {
                        return;
                    }

                    var name = declarator.Names.First();
                    if (name?.ArrayBounds == null ||
                        name.ArrayBounds.Arguments.Count != 1)
                    {
                        return;
                    }

                    var bound = GetConstantArgumentValue(name.ArrayBounds.Arguments.First(), c.SemanticModel);
                    if (!bound.HasValue)
                    {
                        return;
                    }

                    if (!(c.SemanticModel.GetDeclaredSymbol(name) is ILocalSymbol variableSymbol) ||
                        !(variableSymbol.Type is IArrayTypeSymbol))
                    {
                        return;
                    }

                    var statements = GetFollowingStatements(declaration);
                    var indexes = GetAssignedIndexes(statements, variableSymbol, c.SemanticModel).ToHashSet();

                    var upperBound = Math.Max(bound.Value, 0);

                    if (Enumerable.Range(0, upperBound + 1).Any(index => !indexes.Contains(index)))
                    {
                        return;
                    }

                    c.ReportIssue(CreateDiagnostic(rule, declaration.GetLocation()));
                },
                SyntaxKind.LocalDeclarationStatement);
        }

        private static IEnumerable<int> GetAssignedIndexes(IEnumerable<StatementSyntax> statements, ILocalSymbol symbol, SemanticModel semanticModel)
        {
            foreach (var statement in statements)
            {
                if (!(statement is AssignmentStatementSyntax assignment))
                {
                    yield break;
                }

                var invocation = assignment.Left as InvocationExpressionSyntax;
                if (invocation?.ArgumentList == null ||
                    invocation.ArgumentList.Arguments.Count != 1)
                {
                    yield break;
                }

                var assignedSymbol = semanticModel.GetSymbolInfo(invocation.Expression).Symbol;
                if (!symbol.Equals(assignedSymbol))
                {
                    yield break;
                }

                var argument = invocation.ArgumentList.Arguments.First();
                var index = GetConstantArgumentValue(argument, semanticModel);
                if (!index.HasValue)
                {
                    yield break;
                }

                yield return index.Value;
            }
        }

        private static IEnumerable<StatementSyntax> GetFollowingStatements(SyntaxNode node)
        {
            var siblings = node.Parent.ChildNodes().ToList();
            var index = siblings.IndexOf(node);
            if (index < 0)
            {
                yield break;
            }

            for (var i = index + 1; i < siblings.Count; i++)
            {
                if (siblings[i] is StatementSyntax statement)
                {
                    yield return statement;
                }
                else
                {
                    yield break;
                }
            }
        }

        private static int? GetConstantArgumentValue(ArgumentSyntax argument, SemanticModel semanticModel)
        {
            var bound = semanticModel.GetConstantValue(argument.GetExpression());
            if (!bound.HasValue)
            {
                return null;
            }

            var boundValue = bound.Value as int?;
            return boundValue;
        }
    }
}
