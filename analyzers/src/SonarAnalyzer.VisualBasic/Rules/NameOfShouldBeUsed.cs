/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
    public sealed class NameOfShouldBeUsed : NameOfShouldBeUsedBase<MethodBlockBaseSyntax, SyntaxKind, ThrowStatementSyntax>
    {
        private static readonly HashSet<SyntaxKind> StringTokenTypes = new HashSet<SyntaxKind>
        {
            SyntaxKind.InterpolatedStringTextToken,
            SyntaxKind.StringLiteralToken
        };

        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override bool IsStringLiteral(SyntaxToken t) => t.IsAnyKind(StringTokenTypes);

        protected override IEnumerable<string> GetParameterNames(MethodBlockBaseSyntax method)
        {
            var paramGroups = method?.BlockStatement.ParameterList?.Parameters.GroupBy(p => p.Identifier.Identifier.ValueText);
            if (paramGroups == null || paramGroups.Any(g => g.Count() != 1))
            {
                return Enumerable.Empty<string>();
            }

            return paramGroups.Select(g => g.First().Identifier.Identifier.ValueText);
        }

        protected override bool LeastLanguageVersionMatches(SyntaxNodeAnalysisContext context) =>
            context.Compilation.IsAtLeastLanguageVersion(LanguageVersion.VisualBasic14);

        protected override bool IsArgumentExceptionCallingNameOf(SyntaxNode node, IEnumerable<string> arguments)
        {
            var throwNode = (ThrowStatementSyntax)node;
            if (throwNode.Expression is ObjectCreationExpressionSyntax objectCreation)
            {
                var exceptionType = objectCreation.Type.ToString();
                return ArgumentExceptionNameOfPosition(exceptionType) is var idx
                    && objectCreation.ArgumentList.Arguments.Count >= idx + 1
                    && objectCreation.ArgumentList.Arguments[idx].GetExpression() is NameOfExpressionSyntax nameOfExpression
                    && arguments.Contains(nameOfExpression.Argument.ToString());
            }

            return false;
        }
    }
}
