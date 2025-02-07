/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ExpectedExceptionAttributeShouldNotBeUsed : ExpectedExceptionAttributeShouldNotBeUsedBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxNode FindExpectedExceptionAttribute(SyntaxNode node) =>
            ((MethodDeclarationSyntax)node).AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.GetName() is "ExpectedException" or "ExpectedExceptionAttribute");

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
            }
        }
    }
}
