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
    public sealed class DoNotCatchSystemException : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2221";
        private const string MessageFormat = "Catch a list of specific exception subtype or use exception filters instead.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;

                    if (c.AzureFunctionMethod() is null
                        && IsCatchClauseEmptyOrNotPattern(catchClause)
                        && IsSystemException(catchClause.Declaration, c.Model)
                        && !IsThrowTheLastStatementInTheBlock(catchClause.Block))
                    {
                        c.ReportIssue(Rule, GetLocation(catchClause));
                    }
                },
                SyntaxKind.CatchClause);

        private static bool IsCatchClauseEmptyOrNotPattern(CatchClauseSyntax catchClause) =>
            catchClause.Filter?.FilterExpression == null
             || (catchClause.Filter.FilterExpression.IsKind(SyntaxKindEx.IsPatternExpression)
                 && (IsPatternExpressionSyntaxWrapper)catchClause.Filter.FilterExpression is var patternExpression
                 && patternExpression.SyntaxNode.DescendantNodes().AnyOfKind(SyntaxKindEx.NotPattern));

        private static bool IsSystemException(CatchDeclarationSyntax catchDeclaration, SemanticModel semanticModel)
        {
            var caughtTypeSyntax = catchDeclaration?.Type;
            return caughtTypeSyntax == null || semanticModel.GetTypeInfo(caughtTypeSyntax).Type.Is(KnownType.System_Exception);
        }

        private static bool IsThrowTheLastStatementInTheBlock(BlockSyntax block)
        {
            var lastStatement = block?.DescendantNodes()?.LastOrDefault();
            return lastStatement != null && lastStatement.AncestorsAndSelf().TakeWhile(x => !Equals(x, block)).Any(x => x is ThrowStatementSyntax);
        }

        private static Location GetLocation(CatchClauseSyntax catchClause) =>
            catchClause.Declaration?.Type != null
                ? catchClause.Declaration.Type.GetLocation()
                : catchClause.CatchKeyword.GetLocation();
    }
}
