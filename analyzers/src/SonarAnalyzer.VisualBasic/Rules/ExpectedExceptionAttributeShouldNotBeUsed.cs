/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CFG.Extensions;

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class ExpectedExceptionAttributeShouldNotBeUsed : ExpectedExceptionAttributeShouldNotBeUsedBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override SyntaxNode FindExpectedExceptionAttribute(SyntaxNode node) =>
        ((MethodStatementSyntax)node).AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.GetName() is "ExpectedException" or "ExpectedExceptionAttribute");

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

        public override void VisitInvocationExpression(InvocationExpressionSyntax node) =>
            HasAssertion = HasAssertion || node.Expression.ToString().SplitCamelCaseToWords().Intersect(KnownMethods.AssertionMethodParts).Any();
    }
}
