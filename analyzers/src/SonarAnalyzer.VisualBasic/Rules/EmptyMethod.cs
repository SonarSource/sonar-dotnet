/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class EmptyMethod : EmptyMethodBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.FunctionBlock,
            SyntaxKind.SubBlock
        };

        protected override void CheckMethod(SonarSyntaxNodeReportingContext context)
        {
            var methodBlock = (MethodBlockSyntax)context.Node;
            if (methodBlock.Statements.Count == 0
                && !ContainsComments(methodBlock.EndSubOrFunctionStatement.GetLeadingTrivia())
                && !ShouldMethodBeExcluded(context, methodBlock.SubOrFunctionStatement))
            {
                context.ReportIssue(CreateDiagnostic(Rule, methodBlock.SubOrFunctionStatement.Identifier.GetLocation()));
            }
        }

        private static bool ContainsComments(IEnumerable<SyntaxTrivia> trivias) =>
            trivias.Any(s => s.IsKind(SyntaxKind.CommentTrivia));

        private static bool ShouldMethodBeExcluded(SonarSyntaxNodeReportingContext context, MethodStatementSyntax methodStatement)
        {
            if (methodStatement.Modifiers.Any(SyntaxKind.MustOverrideKeyword)
                || methodStatement.Modifiers.Any(SyntaxKind.OverridableKeyword)
                || IsDllImport(context.SemanticModel, methodStatement))
            {
                return true;
            }
            else if (context.SemanticModel.GetDeclaredSymbol(methodStatement) is { IsOverride: true } methodSymbol)
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
