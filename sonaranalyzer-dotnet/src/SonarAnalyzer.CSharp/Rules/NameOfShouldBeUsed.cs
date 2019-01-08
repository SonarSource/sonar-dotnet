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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class NameOfShouldBeUsed : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2302";
        private const string MessageFormat = "Replace the string '{0}' with 'nameof({0})'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly HashSet<SyntaxKind> StringTokenTypes
            = new HashSet<SyntaxKind>
            {
                SyntaxKind.InterpolatedStringTextToken,
                SyntaxKind.StringLiteralToken
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (!c.Compilation.IsAtLeastLanguageVersion(LanguageVersion.CSharp6))
                    {
                        return;
                    }

                    var methodSyntax = (BaseMethodDeclarationSyntax)c.Node;

                    var paramGroups = methodSyntax.ParameterList?.Parameters
                        .GroupBy(p => p.Identifier.ValueText);

                    if (paramGroups != null &&
                        paramGroups.Any(g => g.Count() != 1))
                    {
                        return;
                    }

                    var paramLookup = paramGroups
                        .ToDictionary(g => g.First().Identifier.ValueText,
                                      g => g.First().GetLocation());

                    methodSyntax
                        .DescendantNodes()
                        .OfType<ThrowStatementSyntax>()
                        .SelectMany(th => th.DescendantTokens())
                        .Where(t => t.IsAnyKind(StringTokenTypes))
                        .Where(t => paramLookup.ContainsKey(t.ValueText))
                        .ToList()
                        .ForEach(t => ReportIssue(t, c));
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration);
        }

        private static void ReportIssue(SyntaxToken stringLiteralToken,
            SyntaxNodeAnalysisContext context)
        {
            context.ReportDiagnosticWhenActive(Diagnostic.Create(
                    descriptor: rule,
                    location: stringLiteralToken.GetLocation(),
                    messageArgs: stringLiteralToken.ValueText));
        }
    }
}
