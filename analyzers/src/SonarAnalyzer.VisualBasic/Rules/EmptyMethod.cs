/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class EmptyMethod : EmptyMethodBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override HashSet<SyntaxKind> SyntaxKinds { get; } =
        [
            SyntaxKind.FunctionBlock,
            SyntaxKind.SubBlock
        ];

        protected override void CheckMethod(SonarSyntaxNodeReportingContext context)
        {
            var methodBlock = (MethodBlockSyntax)context.Node;
            if (methodBlock.Statements.Count == 0
                && !ContainsComments(methodBlock.EndSubOrFunctionStatement.GetLeadingTrivia())
                && !ShouldMethodBeExcluded(context, methodBlock.SubOrFunctionStatement))
            {
                context.ReportIssue(Rule, methodBlock.SubOrFunctionStatement.Identifier);
            }
        }

        private static bool ContainsComments(IEnumerable<SyntaxTrivia> trivias) =>
            trivias.Any(s => s.IsKind(SyntaxKind.CommentTrivia));

        private static bool ShouldMethodBeExcluded(SonarSyntaxNodeReportingContext context, MethodStatementSyntax methodStatement)
        {
            if (methodStatement.Modifiers.Any(SyntaxKind.MustOverrideKeyword)
                || methodStatement.Modifiers.Any(SyntaxKind.OverridableKeyword)
                || IsDllImport(context.Model, methodStatement))
            {
                return true;
            }
            else if (context.Model.GetDeclaredSymbol(methodStatement) is { IsOverride: true } methodSymbol)
            {
                return methodSymbol.OverriddenMethod is { IsAbstract: true } || context.IsTestProject();
            }
            else
            {
                return false;
            }
        }

        private static bool IsDllImport(SemanticModel model, MethodStatementSyntax methodStatement) =>
            methodStatement.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.IsKnownType(KnownType.System_Runtime_InteropServices_DllImportAttribute, model));
    }
}
