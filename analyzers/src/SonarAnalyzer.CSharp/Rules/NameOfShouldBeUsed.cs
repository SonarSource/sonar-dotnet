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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class NameOfShouldBeUsed : NameOfShouldBeUsedBase<BaseMethodDeclarationSyntax, SyntaxKind, ThrowStatementSyntax>
    {
        private static readonly HashSet<SyntaxKind> StringTokenTypes = new HashSet<SyntaxKind>
        {
            SyntaxKind.InterpolatedStringTextToken,
            SyntaxKind.StringLiteralToken
        };

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override bool IsStringLiteral(SyntaxToken t) => t.IsAnyKind(StringTokenTypes);

        protected override IEnumerable<string> GetParameterNames(BaseMethodDeclarationSyntax method)
        {
            var paramGroups = method.ParameterList?.Parameters.GroupBy(p => p.Identifier.ValueText);
            if (paramGroups == null || paramGroups.Any(g => g.Count() != 1))
            {
                return Enumerable.Empty<string>();
            }
            return paramGroups.Select(g => g.First().Identifier.ValueText);
        }

        protected override bool LeastLanguageVersionMatches(SyntaxNodeAnalysisContext context) =>
            context.Compilation.IsAtLeastLanguageVersion(LanguageVersion.CSharp6);

        protected override bool IsArgumentExceptionCallingNameOf(SyntaxNode node, IEnumerable<string> arguments)
        {
            var throwNode = (ThrowStatementSyntax)node;
            if (throwNode.Expression is ObjectCreationExpressionSyntax objectCreation)
            {
                var exceptionType = objectCreation.Type.ToString();
                return ArgumentExceptionNameOfPosition(exceptionType) is var idx
                    && objectCreation.ArgumentList?.Arguments is { } creationArguments
                    && creationArguments.Count >= idx + 1
                    && creationArguments[idx].Expression is InvocationExpressionSyntax invocation
                    && invocation.Expression.ToString() == "nameof"
                    && invocation.ArgumentList.Arguments.Count == 1
                    && arguments.Contains(invocation.ArgumentList.Arguments[0].Expression.ToString());
            }

            return false;
        }
    }
}
