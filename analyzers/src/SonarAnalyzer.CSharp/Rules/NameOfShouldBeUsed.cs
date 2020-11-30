/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class NameOfShouldBeUsed : NameOfShouldBeUsedBase<BaseMethodDeclarationSyntax>
    {
       private static readonly HashSet<SyntaxKind> StringTokenTypes
            = new HashSet<SyntaxKind>
            {
                SyntaxKind.InterpolatedStringTextToken,
                SyntaxKind.StringLiteralToken
            };

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override DiagnosticDescriptor Rule { get; } = rule;

        protected override bool IsCaseSensitive => true;

        protected override bool IsStringLiteral(SyntaxToken t) => t.IsAnyKind(StringTokenTypes);

        protected override IEnumerable<string> GetParameterNames(BaseMethodDeclarationSyntax method)
        {
            var paramGroups = method.ParameterList?.Parameters
                .GroupBy(p => p.Identifier.ValueText);

            if (paramGroups != null &&
                paramGroups.Any(g => g.Count() != 1))
            {
                return Enumerable.Empty<string>();
            }

            return paramGroups.Select(g => g.First().Identifier.ValueText);
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (!c.Compilation.IsAtLeastLanguageVersion(LanguageVersion.CSharp6))
                    {
                        return;
                    }

                    ReportIssues<ThrowStatementSyntax>(c);
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration);
        }
    }
}
