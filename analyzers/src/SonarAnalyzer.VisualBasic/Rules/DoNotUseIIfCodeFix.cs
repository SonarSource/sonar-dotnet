/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class DoNotUseIIfCodeFix : SonarCodeFix
    {
        internal const string IfOperatorName = "If";
        internal const string Title = "Use 'If' operator.";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DoNotUseIIf.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (root.FindNode(diagnosticSpan) is InvocationExpressionSyntax iifInvocation)
            {
                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var ifInvocation = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(IfOperatorName), iifInvocation.ArgumentList);

                        var newRoot = root.ReplaceNode(iifInvocation, ifInvocation);

                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }
    }
}
