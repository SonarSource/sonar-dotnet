/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Helpers
{
    public class VisualBasicElementAccessTracker : ElementAccessTracker<SyntaxKind>
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } = VisualBasicGeneratedCodeRecognizer.Instance;
        protected override SyntaxKind[] TrackedSyntaxKinds { get; } = new[] { SyntaxKind.InvocationExpression };

        public VisualBasicElementAccessTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) : base(analyzerConfiguration, rule) { }

        public override ElementAccessCondition ArgumentAtIndexEquals(int index, string value) =>
            context => ((InvocationExpressionSyntax)context.Expression).ArgumentList is { } argumentList
                        && index < argumentList.Arguments.Count
                        && argumentList.Arguments[index].GetExpression().FindStringConstant(context.SemanticModel) == value;

        public override ElementAccessCondition MatchSetter() =>
            context => ((ExpressionSyntax)context.Expression).IsLeftSideOfAssignment();

        public override ElementAccessCondition MatchProperty(MemberDescriptor member) =>
            context => ((InvocationExpressionSyntax)context.Expression).Expression is MemberAccessExpressionSyntax memberAccess
                        && memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                        && memberAccess.Name is SimpleNameSyntax memberName
                        && memberName.Identifier is SyntaxToken memberIdentifier
                        && context.SemanticModel.GetTypeInfo(memberAccess.Expression) is TypeInfo enclosingClassType
                        && memberIdentifier.ValueText != null
                        && enclosingClassType.Type != null
                        && member.IsMatch(memberIdentifier.ValueText, enclosingClassType.Type, caseInsensitiveComparison: true);
    }
}
