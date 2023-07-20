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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnnecessaryBitwiseOperation : UnnecessaryBitwiseOperationBase
    {
        protected override ILanguageFacade Language => CSharpFacade.Instance;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => CheckBinary(c, -1),
                SyntaxKind.BitwiseAndExpression);

            context.RegisterNodeAction(
                c => CheckBinary(c, 0),
                SyntaxKind.BitwiseOrExpression,
                SyntaxKind.ExclusiveOrExpression);

            context.RegisterNodeAction(
                c => CheckAssignment(c, -1),
                SyntaxKind.AndAssignmentExpression);

            context.RegisterNodeAction(
                c => CheckAssignment(c, 0),
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression);
        }

        private void CheckAssignment(SonarSyntaxNodeReportingContext context, int constValueToLookFor)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            if (FindIntConstant(context.SemanticModel, assignment.Right) is { } constValue
                && constValue == constValueToLookFor)
            {
                var location = assignment.Parent is StatementSyntax
                    ? assignment.Parent.GetLocation()
                    : assignment.OperatorToken.CreateLocation(assignment.Right);
                context.ReportIssue(CreateDiagnostic(Rule, location));
            }
        }

        private void CheckBinary(SonarSyntaxNodeReportingContext context, int constValueToLookFor)
        {
            var binary = (BinaryExpressionSyntax)context.Node;
            CheckBinary(context, binary.Left, binary.OperatorToken, binary.Right, constValueToLookFor);
        }
    }
}
