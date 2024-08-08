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

using SonarAnalyzer.Common.Walkers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ExpectedExceptionAttributeShouldNotBeUsed : ExpectedExceptionAttributeShouldNotBeUsedBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override bool HasMultiLineBody(SyntaxNode node)
        {
            var declaration = (MethodDeclarationSyntax)node;
            return declaration.ExpressionBody is null
                && declaration.Body?.Statements.Count > 1;
        }

        protected override bool AssertInCatchFinallyBlock(SyntaxNode node)
        {
            var walker = new CatchFinallyAssertion();
            foreach (var x in node.DescendantNodes().Where(x => x.Kind() is SyntaxKind.CatchClause or SyntaxKind.FinallyClause))
            {
                if (!walker.HasAssertion)
                {
                    walker.SafeVisit(x);
                }
            }
            return walker.HasAssertion;
        }

        private sealed class CatchFinallyAssertion : SafeCSharpSyntaxWalker
        {
            public bool HasAssertion { get; set; }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (HasAssertion)
                {
                    return;
                }

                HasAssertion = node.Expression
                    .ToString()
                    .SplitCamelCaseToWords()
                    .Intersect(UnitTestHelper.KnownAssertionMethodParts)
                    .Any();

                base.VisitInvocationExpression(node);
            }
        }
    }
}
