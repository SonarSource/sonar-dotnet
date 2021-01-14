/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Helpers
{
    public class VisualBasicInvocationTracker : InvocationTracker<SyntaxKind>
    {
        public VisualBasicInvocationTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) : base(analyzerConfiguration, rule, caseInsensitiveComparison: true) { }

        protected override SyntaxKind[] TrackedSyntaxKinds { get; } = new[] { SyntaxKind.InvocationExpression };
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } = VisualBasicGeneratedCodeRecognizer.Instance;

        public override InvocationCondition ArgumentAtIndexIsConstant(int index) =>
            context => ((InvocationExpressionSyntax)context.Invocation).ArgumentList is { } argumentList
                    && argumentList.Arguments.Count > index
                    && argumentList.Arguments[index].GetExpression().HasConstantValue(context.SemanticModel);

        public override InvocationCondition ArgumentAtIndexIsAny(int index, params string[] values) =>
            context => ((InvocationExpressionSyntax)context.Invocation).ArgumentList is { } argumentList
                       && index < argumentList.Arguments.Count
                       && values.Contains(argumentList.Arguments[index].GetExpression().FindStringConstant(context.SemanticModel));

        public override InvocationCondition MatchProperty(MemberDescriptor member) =>
            context => ((InvocationExpressionSyntax)context.Invocation).Expression is MemberAccessExpressionSyntax methodMemberAccess
                    && methodMemberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                    && methodMemberAccess.Expression is MemberAccessExpressionSyntax propertyMemberAccess
                    && propertyMemberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                    && context.SemanticModel.GetTypeInfo(propertyMemberAccess.Expression) is TypeInfo enclosingClassType
                    && member.IsMatch(propertyMemberAccess.Name.Identifier.ValueText, enclosingClassType.Type, caseInsensitiveComparison: true);

        internal override object ConstArgumentForParameter(InvocationContext context, string parameterName)
        {
            var argumentList = ((InvocationExpressionSyntax)context.Invocation).ArgumentList;
            var values = VisualBasicSyntaxHelper.ArgumentValuesForParameter(context.SemanticModel, argumentList, parameterName);
            return values.Length == 1 && values[0] is ExpressionSyntax valueSyntax
                ? valueSyntax.FindConstantValue(context.SemanticModel)
                : null;
        }

        protected override string GetMethodName(SyntaxNode invocationExpression) =>
          ((InvocationExpressionSyntax)invocationExpression).Expression.GetIdentifier()?.Identifier.ValueText;
    }
}
