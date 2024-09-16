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

using SonarAnalyzer.Core.Trackers;
using SonarAnalyzer.VisualBasic.Core.Trackers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class JwtSigned : JwtSignedBase<SyntaxKind, InvocationExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        public JwtSigned() : base(AnalyzerConfiguration.AlwaysEnabled) { }

        protected override BuilderPatternCondition<SyntaxKind, InvocationExpressionSyntax> CreateBuilderPatternCondition() =>
            new VisualBasicBuilderPatternCondition(JwtBuilderConstructorIsSafe, JwtBuilderDescriptors(
                invocation =>
                    invocation.ArgumentList?.Arguments.Count != 1
                    || !invocation.ArgumentList.Arguments.Single().GetExpression().RemoveParentheses().IsKind(SyntaxKind.FalseLiteralExpression)));
    }
}
