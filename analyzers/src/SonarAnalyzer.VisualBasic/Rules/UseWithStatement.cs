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
    public sealed class UseWithStatement : ParametrizedDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2375";
        private const string MessageFormat = "Wrap this and the following {0} statement{2} that use '{1}' in a 'With' statement.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const int DefaultMinimumSeriesLength = 6;

        [RuleParameter("minimumSeriesLength", PropertyType.Integer,
            "Minimum length a series must have to trigger an issue.", DefaultMinimumSeriesLength)]
        public int MinimumSeriesLength { get; set; } = DefaultMinimumSeriesLength;

        private static readonly ISet<Type> WhiteListedStatementTypes = new HashSet<Type>
        {
            typeof(AssignmentStatementSyntax),
            typeof(WithBlockSyntax),
            typeof(ExpressionStatementSyntax)
        };

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    if (MinimumSeriesLength <= 1)
                    {
                        return;
                    }

                    var simpleMemberAccess = (MemberAccessExpressionSyntax)c.Node;

                    var parenthesized = simpleMemberAccess.GetSelfOrTopParenthesizedExpression();
                    if (parenthesized.Parent is MemberAccessExpressionSyntax)
                    {
                        // Only process top level member access expressions
                        return;
                    }

                    var currentMemberExpression = simpleMemberAccess.Expression.RemoveParentheses();
                    while (simpleMemberAccess != null &&
                        !CheckExpression(c, currentMemberExpression))
                    {
                        simpleMemberAccess = currentMemberExpression as MemberAccessExpressionSyntax;
                        currentMemberExpression = simpleMemberAccess?.Expression.RemoveParentheses();
                    }
                },
                SyntaxKind.SimpleMemberAccessExpression);
        }

        private bool CheckExpression(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression)
        {
            if (!IsCandidateForExtraction(context, expression))
            {
                return false;
            }

            var executableStatement = GetParentStatement(expression);
            if (executableStatement == null)
            {
                return false;
            }

            // check previous statement if it contains
            var prevStatement = executableStatement.GetPrecedingStatement() as ExecutableStatementSyntax;
            if (IsCandidateStatement(prevStatement, expression))
            {
                return false;
            }

            var matchCount = 1;

            // check following statements
            var nextStatement = executableStatement.GetSucceedingStatement() as ExecutableStatementSyntax;
            while (IsCandidateStatement(nextStatement, expression))
            {
                matchCount++;
                nextStatement = nextStatement.GetSucceedingStatement() as ExecutableStatementSyntax;
            }

            if (matchCount >= MinimumSeriesLength)
            {
                var nextStatementCount = matchCount - 1;
                context.ReportIssue(CreateDiagnostic(rule, executableStatement.GetLocation(),
                    nextStatementCount, expression.ToString(), nextStatementCount == 1 ? string.Empty : "s"));
                return true;
            }

            return false;
        }

        private static bool IsCandidateForExtraction(SonarSyntaxNodeReportingContext context, ExpressionSyntax currentMemberExpression)
        {
            return currentMemberExpression != null &&
                !currentMemberExpression.IsKind(SyntaxKind.IdentifierName) &&
                !(currentMemberExpression is InstanceExpressionSyntax) &&
                !(context.SemanticModel.GetSymbolInfo(currentMemberExpression).Symbol is ITypeSymbol);
        }

        private static ExecutableStatementSyntax GetParentStatement(ExpressionSyntax expression)
        {
            var expr = expression;
            var parent = expr.Parent as ExpressionSyntax;
            while (parent != null)
            {
                expr = parent;
                parent = expr.Parent as ExpressionSyntax;
            }

            return expr.Parent as ExecutableStatementSyntax;
        }

        private static bool IsCandidateStatement(ExecutableStatementSyntax statement, ExpressionSyntax expression)
        {
            return statement != null &&
                IsAllowedStatement(statement) &&
                !ContainsEmptyMemberAccess(statement) &&
                ContainsExpression(statement, expression);
        }

        private static bool ContainsEmptyMemberAccess(ExecutableStatementSyntax container)
        {
            return container.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Any(m => m.Expression == null);
        }

        private static bool IsAllowedStatement(ExecutableStatementSyntax statement)
        {
            return WhiteListedStatementTypes.Contains(statement.GetType());
        }

        private static bool ContainsExpression(ExecutableStatementSyntax container, ExpressionSyntax contained)
        {
            return contained != null &&
                container.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Any(m => m.Expression != null && VisualBasicEquivalenceChecker.AreEquivalent(contained, m.Expression));
        }
    }
}
