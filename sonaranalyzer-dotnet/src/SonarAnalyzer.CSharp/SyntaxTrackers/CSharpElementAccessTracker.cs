/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public class CSharpElementAccessTracker : ElementAccessTracker<SyntaxKind>
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            CSharp.CSharpGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind[] TrackedSyntaxKinds { get; } =
            new[]
            {
                SyntaxKind.ElementAccessExpression,
            };

        public CSharpElementAccessTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule)
            : base(analyzerConfiguration, rule)
        {
        }

        public override ElementAccessCondition ArgumentAtIndexEquals(int index, string value) =>
            (context) =>
            {
                var argumentList = ((ElementAccessExpressionSyntax)context.Expression).ArgumentList;
                if (argumentList == null ||
                    argumentList.Arguments.Count <= index)
                {
                    return false;
                }
                var constantValue = context.SemanticModel.GetConstantValue(argumentList.Arguments[index].Expression);
                return constantValue.HasValue &&
                    constantValue.Value is string constant &&
                    constant == value;
            };

        #region Syntax-level checking methods

        public override ElementAccessCondition MatchSetter() =>
            (context) => ((ExpressionSyntax)context.Expression).IsLeftSideOfAssignment();

        public override ElementAccessCondition MatchProperty(MemberDescriptor member) =>
            (context) =>
                ((ElementAccessExpressionSyntax)context.Expression).Expression is MemberAccessExpressionSyntax memberAccess &&
                 memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                 memberAccess.Name is SimpleNameSyntax memberName &&
                 memberName.Identifier is SyntaxToken memberIdentifier &&
                 context.SemanticModel.GetTypeInfo(memberAccess.Expression) is TypeInfo enclosingClassType &&
                 memberIdentifier.ValueText != null &&
                 enclosingClassType.Type != null &&
                 member.IsMatch(memberIdentifier.ValueText, enclosingClassType.Type);

        #endregion
    }
}
