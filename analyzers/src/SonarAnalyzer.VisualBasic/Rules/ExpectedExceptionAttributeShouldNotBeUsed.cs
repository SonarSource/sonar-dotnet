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

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class ExpectedExceptionAttributeShouldNotBeUsed : ExpectedExceptionAttributeShouldNotBeUsedBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool HasMultiLineBody(SyntaxNode node) =>
        node.Parent is MethodBlockSyntax { Statements.Count: > 1 };

    protected override bool AssertInCatchFinallyBlock(SyntaxNode node)
    {
        var walker = new CatchFinallyAssertion();
        foreach (var x in node.Parent.DescendantNodes().Where(x => x.Kind() is SyntaxKind.CatchBlock or SyntaxKind.FinallyBlock))
        {
            if (!walker.HasAssertion)
            {
                walker.SafeVisit(x);
            }
        }
        return walker.HasAssertion;
    }

    private sealed class CatchFinallyAssertion : SafeVisualBasicSyntaxWalker
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
        }
    }
}
