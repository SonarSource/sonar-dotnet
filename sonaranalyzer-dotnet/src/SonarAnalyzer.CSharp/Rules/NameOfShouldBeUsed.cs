/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly List<SyntaxKind> StringTokenTypes
            = new List<SyntaxKind>
            {
                SyntaxKind.InterpolatedStringTextToken,
                SyntaxKind.StringLiteralToken
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var methodSyntax = (BaseMethodDeclarationSyntax)c.Node;

                    var paramGroups = methodSyntax.ParameterList.Parameters
                        .GroupBy(p => p.Identifier.ValueText);

                    if (paramGroups.Any(g => g.Skip(1).Any()))
                    {
                        return;
                    }

                    var paramLookup = paramGroups
                        .ToDictionary(g => g.Single().Identifier.ValueText,
                                      g => g.Single().GetLocation());

                    var childTokens = methodSyntax
                        .DescendantNodes()
                        .OfType<ThrowStatementSyntax>()
                        .SelectMany(th => th.DescendantTokens())
                        .Where(t => t.IsAnyKind(StringTokenTypes))
                        .Where(t => paramLookup.ContainsKey(t.ValueText))
                        .ToArray();

                    foreach (var stringLiteralToken in childTokens)
                    {
                        var literalText = stringLiteralToken.ValueText;

                        c.ReportDiagnosticWhenActive(Diagnostic.Create(
                            descriptor: rule,
                            location: stringLiteralToken.GetLocation(),
                            messageArgs: literalText));
                    }
                },
                SyntaxKind.MethodDeclaration);
        }
    }
}
