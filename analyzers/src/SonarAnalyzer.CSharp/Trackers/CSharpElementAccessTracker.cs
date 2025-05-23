﻿/*
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

namespace SonarAnalyzer.Helpers.Trackers
{
    public class CSharpElementAccessTracker : ElementAccessTracker<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override SyntaxKind[] TrackedSyntaxKinds { get; } = new[] { SyntaxKind.ElementAccessExpression };

        public override Condition ArgumentAtIndexEquals(int index, string value) =>
            context => ((ElementAccessExpressionSyntax)context.Node).ArgumentList is { } argumentList
                       && index < argumentList.Arguments.Count
                       && argumentList.Arguments[index].Expression.FindStringConstant(context.SemanticModel) == value;

        public override Condition MatchSetter() =>
            context => ((ExpressionSyntax)context.Node).IsLeftSideOfAssignment();

        public override Condition MatchProperty(MemberDescriptor member) =>
            context => ((ElementAccessExpressionSyntax)context.Node).Expression is MemberAccessExpressionSyntax memberAccess
                       && memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                       && context.SemanticModel.GetTypeInfo(memberAccess.Expression) is TypeInfo enclosingClassType
                       && member.IsMatch(memberAccess.Name.Identifier.ValueText, enclosingClassType.Type, Language.NameComparison);
    }
}
