/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class EmptyMethod : EmptyMethodBase<SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            Helpers.VisualBasic.VisualBasicGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } = new []
        {
            SyntaxKind.FunctionBlock,
            SyntaxKind.SubBlock
        };

        protected override void CheckMethod(SyntaxNodeAnalysisContext context)
        {
            var methodBlock = (MethodBlockSyntax)context.Node;

            if (methodBlock.Statements.Count == 0 &&
                !ContainsComments(methodBlock.EndSubOrFunctionStatement.GetLeadingTrivia()) &&
                !ShouldMethodBeExcluded(methodBlock.SubOrFunctionStatement, context.SemanticModel))
            {
                context.ReportDiagnosticWhenActive(
                    Diagnostic.Create(rule, methodBlock.SubOrFunctionStatement.Identifier.GetLocation()));
            }
        }

        private static bool ContainsComments(IEnumerable<SyntaxTrivia> trivias)
            => trivias.Any(s => s.IsKind(SyntaxKind.CommentTrivia));

        private static bool ShouldMethodBeExcluded(MethodStatementSyntax methodStatement, SemanticModel semanticModel)
        {
            if (methodStatement.Modifiers.Any(SyntaxKind.MustOverrideKeyword) ||
                methodStatement.Modifiers.Any(SyntaxKind.OverridableKeyword))
            {
                return true;
            }

            if (IsDllImport(methodStatement))
            {
                return true;
            }

            var methodSymbol = semanticModel.GetDeclaredSymbol(methodStatement);
            if (methodSymbol != null &&
                methodSymbol.IsOverride &&
                methodSymbol.OverriddenMethod != null &&
                methodSymbol.OverriddenMethod.IsMustOverride())
            {
                return true;
            }

            return methodSymbol.IsOverrides() && semanticModel.Compilation.IsTest();
        }

        private static bool IsDllImport(MethodStatementSyntax methodStatement) => methodStatement.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(a => a.Name.GetText().ToString().Equals("dllimport", System.StringComparison.OrdinalIgnoreCase));
    }
}
